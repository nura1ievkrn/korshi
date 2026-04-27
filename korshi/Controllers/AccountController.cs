using korshi.Models;
using korshi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace korshi.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ── GET /Account/Register ────────────────────────
        [HttpGet]
        public IActionResult Register() =>
            User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Home") : View();

        // ── POST /Account/Register ───────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Address = model.Address.Trim(),
                ApartmentNumber = model.ApartmentNumber.Trim(),
                ComplexId = 1,      // пока хардкод — первый ЖК
                IsVerified = false,  // ждёт подтверждения админа
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Resident");
                // НЕ входим сразу — ждём верификации
                return RedirectToAction(nameof(Pending));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // ── GET /Account/Pending ─────────────────────────
        [HttpGet]
        public IActionResult Pending() => View();

        // ── GET /Account/Login ───────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // ── POST /Account/Login ──────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Неверный email или пароль");
                return View(model);
            }

            if (user.IsBanned)
            {
                ModelState.AddModelError(string.Empty, "Ваш аккаунт заблокирован. Обратитесь к администратору.");
                return View(model);
            }

            if (!user.IsVerified)
            {
                ModelState.AddModelError(string.Empty, "Ваш аккаунт ещё не подтверждён председателем ОСИ.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Слишком много попыток. Попробуйте позже.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Неверный email или пароль");
            return View(model);
        }

        // ── POST /Account/Logout ─────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
    }
}