using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GLMS.Models;
using GLMS.Services;

namespace GLMS.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly GlmsApiClient _apiClient;

        public ServiceRequestController(GlmsApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _apiClient.GetServiceRequestsAsync();
            return View(requests);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var request = await _apiClient.GetServiceRequestAsync(id.Value);
            if (request == null) return NotFound();
            return View(request);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateContractDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest request)
        {
            ModelState.Remove("Contract");

            if (ModelState.IsValid)
            {
                var (success, error) = await _apiClient
                    .CreateServiceRequestAsync(request);

                if (!success)
                {
                    ModelState.AddModelError("", error);
                    await PopulateContractDropdown();
                    return View(request);
                }

                return RedirectToAction(nameof(Index));
            }

            await PopulateContractDropdown();
            return View(request);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var request = await _apiClient.GetServiceRequestAsync(id.Value);
            if (request == null) return NotFound();
            await PopulateContractDropdown(request.ContractId);
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceRequest request)
        {
            if (id != request.ServiceRequestId) return NotFound();
            ModelState.Remove("Contract");

            if (ModelState.IsValid)
            {
                await _apiClient.UpdateServiceRequestAsync(request);
                return RedirectToAction(nameof(Index));
            }

            await PopulateContractDropdown(request.ContractId);
            return View(request);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var request = await _apiClient.GetServiceRequestAsync(id.Value);
            if (request == null) return NotFound();
            return View(request);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiClient.DeleteServiceRequestAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateContractDropdown(int? selectedId = null)
        {
            var contracts = await _apiClient.GetContractsAsync(
                status: ContractStatus.Active);

            var items = contracts.Select(c => new
            {
                c.ContractId,
                Display = $"{c.Customer?.Name} — {c.ServiceLevel} " +
                          $"(Expires: {c.EndDate:dd MMM yyyy})"
            }).ToList();

            ViewBag.Contracts = new SelectList(
                items, "ContractId", "Display", selectedId);
        }
    }
}