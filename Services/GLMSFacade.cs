using GLMS.Data;
using GLMS.Models;
using GLMS.Services.Factories;
using GLMS.Services.Observer;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GLMS.Services
{
    public class GLMSFacade
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GLMSFacade> _logger;

        // Observer instances
        private readonly List<IContractObserver> _observers;

        public GLMSFacade(
    ApplicationDbContext context,
    IHttpClientFactory httpClientFactory,
    ILogger<GLMSFacade> logger,
    ILogger<NotificationService> notifLogger,
    ILogger<AuditLogService> auditLogger)
{
    _context = context;
    _httpClientFactory = httpClientFactory;
    _logger = logger;

    _observers = new List<IContractObserver>
    {
        new NotificationService(notifLogger),
        new AuditLogService(auditLogger)
    };
        }
        // ── CONTRACT OPERATIONS ───────────────────────────────────────

        public async Task<Contract> CreateContractAsync(
            string serviceLevel, DateTime startDate,
            DateTime endDate, int customerId, bool isInternational,
            IFormFile? pdfFile, IWebHostEnvironment environment)
        {
            // Factory Method — decide which factory to use
            ContractFactory factory = isInternational
                ? new InternationalContractFactory()
                : new StandardContractFactory();

            var contract = factory.CreateContract(
                serviceLevel, startDate, endDate, customerId);

            // Handle PDF upload
            if (pdfFile != null && pdfFile.Length > 0)
            {
                contract.SignedAgreementPath = await SavePdfAsync(
                    pdfFile, environment);
            }

            _context.Add(contract);
            await _context.SaveChangesAsync();

            // Notify observers of new contract creation
            await contract.ChangeStatus(contract.Status, _observers);

            return contract;
        }

        public async Task UpdateContractStatusAsync(int contractId,
            ContractStatus newStatus)
        {
            var contract = await _context.Contracts.FindAsync(contractId);
            if (contract == null)
                throw new Exception("Contract not found.");

            // Observer pattern fires here
            await contract.ChangeStatus(newStatus, _observers);

            _context.Update(contract);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Contract>> SearchContractsAsync(
            DateTime? startDate, DateTime? endDate, ContractStatus? status)
        {
            var query = _context.Contracts
                .Include(c => c.Customer)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(c => c.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.EndDate <= endDate.Value);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            return await query.ToListAsync();
        }

        // ── SERVICE REQUEST OPERATIONS ────────────────────────────────

        public async Task<(bool success, string error)> CreateServiceRequestAsync(
            ServiceRequest request)
        {
            // Workflow check
            var contract = await _context.Contracts.FindAsync(request.ContractId);
            if (contract == null)
                return (false, "Contract not found.");

            if (contract.Status != ContractStatus.Active)
                return (false,
                    $"Cannot raise a request against a {contract.Status} contract.");
            // Currency conversion
            try
            {
                decimal rate = await GetUsdToZarRateAsync();
                request.CostZAR = Math.Round(request.CostUSD * rate, 2);
            }
            catch
            {
                request.CostZAR = 0;
            }

            request.Status = ServiceRequestStatus.Pending;
            _context.Add(request);
            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }

        // ── FILE HANDLING ─────────────────────────────────────────────

        public async Task<string> SavePdfAsync(IFormFile pdfFile,
            IWebHostEnvironment environment)
        {
            if (Path.GetExtension(pdfFile.FileName).ToLower() != ".pdf")
                throw new InvalidOperationException("Only PDF files are allowed.");

            var uploadsFolder = Path.Combine(
                environment.WebRootPath, "uploads", "contracts");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{pdfFile.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await pdfFile.CopyToAsync(stream);

            return $"uploads/contracts/{uniqueFileName}";
        }

        // ── CURRENCY API ──────────────────────────────────────────────

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(
                "https://open.er-api.com/v6/latest/USD");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            return doc.RootElement
                .GetProperty("rates")
                .GetProperty("ZAR")
                .GetDecimal();
        }
    }
}