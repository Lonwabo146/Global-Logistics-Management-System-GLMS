using GLMS.Data;
using GLMS.Models;
using GLMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Controllers
{
    public class ContractController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly GLMSFacade _facade;
        public ContractController(ApplicationDbContext context, GLMSFacade facade,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
            _facade = facade;
        }

        // GET: Contract — with Search/Filter
        public async Task<IActionResult> Index(DateTime? startDate,
            DateTime? endDate, ContractStatus? status)
        {
            var query = _context.Contracts
                .Include(c => c.Customer)
                .AsQueryable();

            // LINQ filtering
            if (startDate.HasValue)
                query = query.Where(c => c.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.EndDate <= endDate.Value);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            // Pass filter values back to view to keep form filled in
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;
            ViewBag.StatusList = new SelectList(
                Enum.GetValues(typeof(ContractStatus)), status);

            return View(await query.ToListAsync());
        }

        // GET: Contract/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts
                .Include(c => c.Customer)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.ContractId == id);

            if (contract == null) return NotFound();

            return View(contract);
        }

        // GET: Contract/Create
        public IActionResult Create()
        {
            ViewBag.Customers = new SelectList(
                _context.Customer, "CustomerId", "Name");
            return View();
        }

        // POST: Contract/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract,
    IFormFile? pdfFile, string? isInternational = null)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("SignedAgreementPath");

            if (ModelState.IsValid)
            {
                // Validate PDF extension before passing to facade
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    if (Path.GetExtension(pdfFile.FileName).ToLower() != ".pdf")
                    {
                        ModelState.AddModelError("pdfFile",
                            "Only PDF files are allowed.");
                        ViewBag.Customers = new SelectList(
                            _context.Customer, "CustomerId", "Name");
                        return View(contract);
                    }
                }

                try
                {
                    // Facade handles Factory + Observer + file save + DB save
                    await _facade.CreateContractAsync(
                        contract.ServiceLevel,
                        contract.StartDate,
                        contract.EndDate,
                        contract.CustomerId,
                        isInternational == "true",
                        pdfFile,
                        _environment);

                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("pdfFile", ex.Message);
                }
            }

            ViewBag.Customers = new SelectList(
                _context.Customer, "CustomerId", "Name");
            return View(contract);
        }

        // GET: Contract/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            ViewBag.Customers = new SelectList(
                _context.Customer, "CustomerId", "Name", contract.CustomerId);
            return View(contract);
        }

        // POST: Contract/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract,
            IFormFile? pdfFile)
        {
            if (id != contract.ContractId) return NotFound();

            ModelState.Remove("Customer");
            ModelState.Remove("SignedAgreementPath");

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle new PDF upload on edit
                    if (pdfFile != null && pdfFile.Length > 0)
                    {
                        if (Path.GetExtension(pdfFile.FileName).ToLower() != ".pdf")
                        {
                            ModelState.AddModelError("pdfFile",
                                "Only PDF files are allowed.");
                            ViewBag.Customers = new SelectList(
                                _context.Customer, "CustomerId", "Name");
                            return View(contract);
                        }

                        var uploadsFolder = Path.Combine(
                            _environment.WebRootPath, "uploads", "contracts");
                        Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = $"{Guid.NewGuid()}_{pdfFile.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await pdfFile.CopyToAsync(stream);
                        }

                        contract.SignedAgreementPath =
                            $"uploads/contracts/{uniqueFileName}";
                    }
                    else
                    {
                        // Keep existing file path if no new file uploaded
                        var existing = await _context.Contracts
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.ContractId == id);
                        contract.SignedAgreementPath = existing?.SignedAgreementPath;
                    }

                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Contracts.Any(c => c.ContractId == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = new SelectList(
                _context.Customer, "CustomerId", "Name");
            return View(contract);
        }

        // GET: Contract/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.ContractId == id);

            if (contract == null) return NotFound();

            return View(contract);
        }

        // POST: Contract/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Contract/Download/5
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || contract.SignedAgreementPath == null)
                return NotFound();

            var filePath = Path.Combine(
                _environment.WebRootPath, contract.SignedAgreementPath);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf",
                Path.GetFileName(filePath));
        }
    }
}

