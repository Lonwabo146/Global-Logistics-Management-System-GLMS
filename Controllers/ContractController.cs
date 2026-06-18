using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GLMS.Models;
using GLMS.Services;

namespace GLMS.Controllers
{
    public class ContractController : Controller
    {
        private readonly GlmsApiClient _apiClient;
        private readonly IWebHostEnvironment _environment;

        public ContractController(GlmsApiClient apiClient,
            IWebHostEnvironment environment)
        {
            _apiClient = apiClient;
            _environment = environment;
        }

        public async Task<IActionResult> Index(DateTime? startDate,
            DateTime? endDate, ContractStatus? status)
        {
            var contracts = await _apiClient.GetContractsAsync(
                startDate, endDate, status);

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;
            ViewBag.StatusList = new SelectList(
                Enum.GetValues(typeof(ContractStatus)), status);

            return View(contracts);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var contract = await _apiClient.GetContractAsync(id.Value);
            if (contract == null) return NotFound();
            return View(contract);
        }

        public async Task<IActionResult> Create()
        {
            var customers = await _apiClient.GetCustomersAsync();
            ViewBag.Customers = new SelectList(
                customers, "CustomerId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract,
            IFormFile? pdfFile, string? isInternational = null)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("SignedAgreementPath");

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate PDF before sending to API
                    string? pdfPath = null;
                    if (pdfFile != null && pdfFile.Length > 0)
                    {
                        if (Path.GetExtension(pdfFile.FileName).ToLower() != ".pdf")
                        {
                            ModelState.AddModelError("pdfFile",
                                "Only PDF files are allowed.");
                            var customerList = await _apiClient.GetCustomersAsync();
                            ViewBag.Customers = new SelectList(
                                customerList, "CustomerId", "Name");
                            return View(contract);
                        }

                        // Save PDF locally in MVC project
                        var uploadsFolder = Path.Combine(
                            _environment.WebRootPath, "uploads", "contracts");
                        Directory.CreateDirectory(uploadsFolder);
                        var uniqueFileName = $"{Guid.NewGuid()}_{pdfFile.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using var stream = new FileStream(filePath, FileMode.Create);
                        await pdfFile.CopyToAsync(stream);
                        pdfPath = $"uploads/contracts/{uniqueFileName}";
                    }

                    // Create contract via API
                    var created = await _apiClient.CreateContractAsync(
                        contract.ServiceLevel,
                        contract.StartDate,
                        contract.EndDate,
                        contract.CustomerId,
                        isInternational == "true");

                    // If PDF was uploaded update the path via PUT
                    if (created != null && pdfPath != null)
                    {
                        created.SignedAgreementPath = pdfPath;
                        await _apiClient.UpdateContractAsync(created);
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var customers = await _apiClient.GetCustomersAsync();
            ViewBag.Customers = new SelectList(
                customers, "CustomerId", "Name");
            return View(contract);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var contract = await _apiClient.GetContractAsync(id.Value);
            if (contract == null) return NotFound();

            var customers = await _apiClient.GetCustomersAsync();
            ViewBag.Customers = new SelectList(
                customers, "CustomerId", "Name", contract.CustomerId);
            return View(contract);
        }

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
                    // Handle PDF upload
                    if (pdfFile != null && pdfFile.Length > 0)
                    {
                        if (Path.GetExtension(pdfFile.FileName).ToLower() != ".pdf")
                        {
                            ModelState.AddModelError("pdfFile",
                                "Only PDF files are allowed.");
                            var customerList = await _apiClient.GetCustomersAsync();
                            ViewBag.Customers = new SelectList(
                                customerList, "CustomerId", "Name");
                            return View(contract);
                        }

                        var uploadsFolder = Path.Combine(
                            _environment.WebRootPath, "uploads", "contracts");
                        Directory.CreateDirectory(uploadsFolder);
                        var uniqueFileName = $"{Guid.NewGuid()}_{pdfFile.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using var stream = new FileStream(filePath, FileMode.Create);
                        await pdfFile.CopyToAsync(stream);
                        contract.SignedAgreementPath =
                            $"uploads/contracts/{uniqueFileName}";
                    }

                    // Update status via PATCH — triggers Observer
                    await _apiClient.UpdateContractStatusAsync(
                        id, contract.Status);

                    // Update full contract via PUT
                    await _apiClient.UpdateContractAsync(contract);

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var customers = await _apiClient.GetCustomersAsync();
            ViewBag.Customers = new SelectList(
                customers, "CustomerId", "Name");
            return View(contract);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var contract = await _apiClient.GetContractAsync(id.Value);
            if (contract == null) return NotFound();
            return View(contract);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiClient.DeleteContractAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Download(int? id)
        {
            if (id == null) return NotFound();
            var contract = await _apiClient.GetContractAsync(id.Value);
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