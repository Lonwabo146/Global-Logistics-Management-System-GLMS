using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Data;
using GLMS.Models;
using GLMS.Services;

namespace GLMS.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GLMSFacade _facade;

        public ServiceRequestController(ApplicationDbContext context,
            GLMSFacade facade)
        {
            _context = context;
            _facade = facade;
        }

        // GET: ServiceRequest
        public async Task<IActionResult> Index()
        {
            var requests = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .ThenInclude(c => c.Customer)
                .ToListAsync();

            return View(requests);
        }

        // GET: ServiceRequest/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .ThenInclude(c => c.Customer)
                .FirstOrDefaultAsync(sr => sr.ServiceRequestId == id);

            if (request == null) return NotFound();

            return View(request);
        }

        // GET: ServiceRequest/Create
        public IActionResult Create()
        {
            PopulateContractDropdown();
            return View();
        }

        // POST: ServiceRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest request)
        {
            ModelState.Remove("Contract");

            if (ModelState.IsValid)
            {
                // Workflow + currency conversion go through Facade
                var (success, error) = await _facade
                    .CreateServiceRequestAsync(request);

                if (!success)
                {
                    ModelState.AddModelError("", error);
                    PopulateContractDropdown();
                    return View(request);
                }

                return RedirectToAction(nameof(Index));
            }

            PopulateContractDropdown();
            return View(request);
        }

        // GET: ServiceRequest/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null) return NotFound();

            PopulateContractDropdown(request.ContractId);
            return View(request);
        }

        // POST: ServiceRequest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceRequest request)
        {
            if (id != request.ServiceRequestId) return NotFound();

            ModelState.Remove("Contract");

            if (ModelState.IsValid)
            {
                try
                {
                    // Recalculate ZAR through facade
                    decimal rate = await _facade.GetUsdToZarRateAsync();
                    request.CostZAR = Math.Round(request.CostUSD * rate, 2);

                    _context.Update(request);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ServiceRequests
                        .Any(sr => sr.ServiceRequestId == id))
                        return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            PopulateContractDropdown(request.ContractId);
            return View(request);
        }

        // GET: ServiceRequest/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .ThenInclude(c => c.Customer)
                .FirstOrDefaultAsync(sr => sr.ServiceRequestId == id);

            if (request == null) return NotFound();

            return View(request);
        }

        // POST: ServiceRequest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request != null)
            {
                _context.ServiceRequests.Remove(request);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private void PopulateContractDropdown(int? selectedId = null)
        {
            var activeContracts = _context.Contracts
                .Include(c => c.Customer)
                .Where(c => c.Status == ContractStatus.Active)
                .Select(c => new
                {
                    c.ContractId,
                    Display = $"{c.Customer.Name} — {c.ServiceLevel} " +
                              $"(Expires: {c.EndDate:dd MMM yyyy})"
                })
                .ToList();

            ViewBag.Contracts = new SelectList(
                activeContracts, "ContractId", "Display", selectedId);
        }
    }
}