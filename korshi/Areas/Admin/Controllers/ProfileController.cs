using korshi.Models;
using korshi.Services;
using korshi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace korshi.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PostService _postService;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            PostService postService)
        {
            _userManager = userManager;
            _postService = postService;
        }

        // ── GET /Profile ───────────────────────────────────
        public async Task<IActionResult> Index(string? userId = null, string tab = "posts")
        {
            ApplicationUser? profileUser;

            if (string.IsNullOrEmpty(userId))
                profileUser = await _userManager.GetUserAsync(User);
            else
                profileUser = await _userManager.FindByIdAsync(userId);

            if (profileUser == null) return NotFound();

            var myPosts = await _postService.GetUserPostsAsync(profileUser.Id);
            var likedPosts = await _postService.GetLikedPostsAsync(profileUser.Id);

            var vm = new ProfileViewModel
            {
                User = profileUser,
                MyPosts = myPosts,
                LikedPosts = likedPosts,
                ActiveTab = tab
            };

            ViewData["ActiveNav"] = "settings";
            return View(vm);
        }

        // ── GET /Profile/Edit ──────────────────────────────
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var vm = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                ApartmentNumber = user.ApartmentNumber
            };

            ViewData["ActiveNav"] = "settings";
            return View(vm);
        }

        // ── POST /Profile/Edit ─────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ActiveNav"] = "settings";
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName.Trim();
            user.LastName = model.LastName.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();
            user.Address = model.Address?.Trim();
            user.ApartmentNumber = model.ApartmentNumber?.Trim();

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "Профиль успешно обновлён!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ViewData["ActiveNav"] = "settings";
            return View(model);
        }

        // ── GET /Profile/ChangePassword ────────────────────
        public IActionResult ChangePassword()
        {
            ViewData["ActiveNav"] = "settings";
            return View(new ChangePasswordViewModel());
        }

        // ── POST /Profile/ChangePassword ───────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ActiveNav"] = "settings";
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(
                user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "Пароль успешно изменён!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ViewData["ActiveNav"] = "settings";
            return View(model);
        }
    }
}