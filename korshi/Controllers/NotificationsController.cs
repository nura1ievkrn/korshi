using korshi.Models;
using korshi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace korshi.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly NotificationService _notifService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(
            NotificationService notifService,
            UserManager<ApplicationUser> userManager)
        {
            _notifService = notifService;
            _userManager = userManager;
        }

        // GET /Notifications
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var notifications = await _notifService.GetUserNotificationsAsync(user.Id);
            await _notifService.MarkAllReadAsync(user.Id);

            ViewData["ActiveNav"] = "notifications";
            return View(notifications);
        }

        // GET /Notifications/Count — для колокольчика
        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { count = 0 });

            var count = await _notifService.GetUnreadCountAsync(user.Id);
            return Json(new { count });
        }
    }
}