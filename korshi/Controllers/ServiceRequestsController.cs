using korshi.Models;
using korshi.Services;
using korshi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace korshi.Controllers
{
    [Authorize]
    public class ServiceRequestsController : Controller
    {
        private readonly ServiceRequestService _service;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceRequestsController(
            ServiceRequestService service,
            UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        // ── GET /ServiceRequests ──────────────────────────
        public async Task<IActionResult> Index(string filter = "all")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var requests = await _service.GetUserRequestsAsync(user.Id);

            requests = filter switch
            {
                "new" => requests.Where(r => r.Status == ServiceRequestStatus.New).ToList(),
                "inprocess" => requests.Where(r => r.Status == ServiceRequestStatus.InProcess).ToList(),
                "done" => requests.Where(r => r.Status == ServiceRequestStatus.Done).ToList(),
                _ => requests
            };

            ViewData["ActiveNav"] = "requests";
            return View(new ServiceRequestsIndexViewModel
            {
                Requests = requests,
                Filter = filter
            });
        }

        // ── GET /ServiceRequests/Create ───────────────────
        public async Task<IActionResult> Create()
        {
            var services = await _service.GetServicesAsync();
            ViewData["ActiveNav"] = "requests";
            return View(new CreateServiceRequestViewModel { Services = services });
        }

        // ── POST /ServiceRequests/Create ──────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateServiceRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Services = await _service.GetServicesAsync();
                ViewData["ActiveNav"] = "requests";
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _service.CreateAsync(
                model.Title, model.Description, model.ServiceId, user.Id);

            TempData["Success"] = "Заявка успешно отправлена!";
            return RedirectToAction(nameof(Index));
        }

        // ── POST /ServiceRequests/Delete/{id} ─────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var isAdmin = await _userManager.IsInRoleAsync(user, "AdminOSI");
            await _service.DeleteAsync(id, user.Id, isAdmin);

            TempData["Success"] = "Заявка удалена.";
            return RedirectToAction(nameof(Index));
        }
    }
}