using korshi.Data;
using korshi.Models;
using korshi.Services;
using korshi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace korshi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "AdminOSI")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly ServiceRequestService _srService;
        private readonly IWebHostEnvironment _env;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db,
            ServiceRequestService srService,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _db = db;
            _srService = srService;
            _env = env;
        }

        // ── GET /Admin ────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalUsers = await _db.Users.CountAsync(),
                PendingUsers = await _db.Users.CountAsync(u => !u.IsVerified && !u.IsBanned),
                ActiveUsers = await _db.Users.CountAsync(u => u.IsVerified && !u.IsBanned),
                BannedUsers = await _db.Users.CountAsync(u => u.IsBanned),
                TotalPosts = await _db.Posts.CountAsync(p => p.IsActive),
                OpenRequests = await _db.ServiceRequests
                                    .CountAsync(r => r.Status == ServiceRequestStatus.New),
                ActivePolls = await _db.Polls
                                    .CountAsync(p => p.IsActive && p.EndDate > DateTime.UtcNow),

                // Последние 5 ожидающих
                PendingList = await _db.Users
                    .Where(u => !u.IsVerified && !u.IsBanned)
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }

        // ── GET /Admin/Users ──────────────────────────────
        public async Task<IActionResult> Users(string? filter = null)
        {
            var query = _db.Users
                .Include(u => u.Complex)
                .AsQueryable();

            query = filter switch
            {
                "pending" => query.Where(u => !u.IsVerified && !u.IsBanned),
                "active" => query.Where(u => u.IsVerified && !u.IsBanned),
                "banned" => query.Where(u => u.IsBanned),
                _ => query
            };

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            ViewData["Filter"] = filter ?? "all";
            return View(users);
        }

        // ── POST /Admin/Verify/{id} ───────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsVerified = true;
            user.IsBanned = false;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"Пользователь {user.FullName} подтверждён!";
            return RedirectToAction(nameof(Users), new { filter = "pending" });
        }

        // ── POST /Admin/Ban/{id} ──────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ban(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Нельзя забанить другого AdminOSI
            if (await _userManager.IsInRoleAsync(user, "AdminOSI"))
            {
                TempData["Error"] = "Нельзя заблокировать администратора.";
                return RedirectToAction(nameof(Users));
            }

            user.IsBanned = true;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"Пользователь {user.FullName} заблокирован.";
            return RedirectToAction(nameof(Users));
        }

        // ── POST /Admin/Unban/{id} ────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unban(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsBanned = false;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"Пользователь {user.FullName} разблокирован.";
            return RedirectToAction(nameof(Users));
        }

        // ── GET /Admin/ServiceRequests ────────────────────
        public async Task<IActionResult> ServiceRequests()
        {
            var requests = await _srService.GetAllAsync();
            ViewData["AdminNav"] = "requests";
            return View(requests);
        }

        // ── POST /Admin/UpdateStatus ──────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(UpdateStatusViewModel model)
        {
            await _srService.UpdateStatusAsync(model.RequestId, model.Status, model.AdminNote);
            TempData["Success"] = "Статус заявки обновлён.";
            return RedirectToAction(nameof(ServiceRequests));
        }

        // ══════════════════════════════════════════════════
        // БАННЕРЫ
        // ══════════════════════════════════════════════════

        // ── GET /Admin/Banners ────────────────────────────
        public async Task<IActionResult> Banners()
        {
            var banners = await _db.Banners
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            ViewData["AdminNav"] = "banners";
            return View(banners);
        }

        // ── POST /Admin/CreateBanner ──────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBanner(string message, int type, DateTime? expiresAt)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Введите текст баннера.";
                return RedirectToAction(nameof(Banners));
            }

            var user = await _userManager.GetUserAsync(User);

            _db.Banners.Add(new Banner
            {
                Message = message.Trim(),
                Type = (BannerType)type,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt?.ToUniversalTime(),
                UserId = user!.Id
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Баннер опубликован!";
            return RedirectToAction(nameof(Banners));
        }

        // ── POST /Admin/DeleteBanner/{id} ─────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var banner = await _db.Banners.FindAsync(id);
            if (banner != null)
            {
                _db.Banners.Remove(banner);
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "Баннер удалён.";
            return RedirectToAction(nameof(Banners));
        }

        // ══════════════════════════════════════════════════
        // УСЛУГИ
        // ══════════════════════════════════════════════════

        // ── GET /Admin/Services ───────────────────────────
        public async Task<IActionResult> Services()
        {
            var services = await _db.Services
                .Include(s => s.Requests)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
            ViewData["AdminNav"] = "services";
            return View(services);
        }

        // ── POST /Admin/CreateService ─────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(string name, string? description,
            string icon, string colorText)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Введите название услуги.";
                return RedirectToAction(nameof(Services));
            }

            var maxOrder = await _db.Services.MaxAsync(s => (int?)s.SortOrder) ?? 0;

            _db.Services.Add(new Service
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Icon = icon?.Trim() ?? "bi-wrench",
                Color = colorText?.Trim() ?? "#6B7280",
                IsActive = true,
                SortOrder = maxOrder + 1
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Услуга «{name}» добавлена!";
            return RedirectToAction(nameof(Services));
        }

        // ── POST /Admin/ToggleService/{id} ────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleService(int id)
        {
            var service = await _db.Services.FindAsync(id);
            if (service != null)
            {
                service.IsActive = !service.IsActive;
                await _db.SaveChangesAsync();
                TempData["Success"] = service.IsActive ? "Услуга показана." : "Услуга скрыта.";
            }
            return RedirectToAction(nameof(Services));
        }

        // ── POST /Admin/DeleteService/{id} ────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _db.Services.FindAsync(id);
            if (service != null)
            {
                service.IsActive = false;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Услуга удалена.";
            }
            return RedirectToAction(nameof(Services));
        }

        // ══════════════════════════════════════════════════
        // КАТЕГОРИИ
        // ══════════════════════════════════════════════════

        // ── GET /Admin/Categories ─────────────────────────
        public async Task<IActionResult> Categories()
        {
            var categories = await _db.Categories
                .Include(c => c.Posts)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
            ViewData["AdminNav"] = "categories";
            return View(categories);
        }

        // ── POST /Admin/CreateCategory ────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string name, string slug,
            string icon, string color)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(slug))
            {
                TempData["Error"] = "Введите название и slug.";
                return RedirectToAction(nameof(Categories));
            }

            var exists = await _db.Categories.AnyAsync(c => c.Slug == slug.Trim().ToLower());
            if (exists)
            {
                TempData["Error"] = "Категория с таким slug уже существует.";
                return RedirectToAction(nameof(Categories));
            }

            var maxOrder = await _db.Categories.MaxAsync(c => (int?)c.SortOrder) ?? 0;

            _db.Categories.Add(new Category
            {
                Name = name.Trim(),
                Slug = slug.Trim().ToLower(),
                Icon = icon?.Trim() ?? "bi-tag",
                Color = color?.Trim() ?? "#6B7280",
                IsActive = true,
                SortOrder = maxOrder + 1
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Категория «{name}» добавлена!";
            return RedirectToAction(nameof(Categories));
        }

        // ── POST /Admin/ToggleCategory/{id} ───────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategory(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat != null)
            {
                cat.IsActive = !cat.IsActive;
                await _db.SaveChangesAsync();
                TempData["Success"] = cat.IsActive ? "Категория показана." : "Категория скрыта.";
            }
            return RedirectToAction(nameof(Categories));
        }

        // ══════════════════════════════════════════════════
        // СОБЫТИЯ
        // ══════════════════════════════════════════════════

        // ── GET /Admin/Events ─────────────────────────────
        public async Task<IActionResult> Events()
        {
            var events = await _db.Events
                .OrderBy(e => e.EventDate)
                .ToListAsync();
            ViewData["AdminNav"] = "events";
            return View(events);
        }

        // ── POST /Admin/CreateEvent ───────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(string title, string? description, DateTime eventDate)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = "Введите название события.";
                return RedirectToAction(nameof(Events));
            }

            var user = await _userManager.GetUserAsync(User);

            _db.Events.Add(new Event
            {
                Title = title.Trim(),
                Description = description?.Trim(),
                EventDate = eventDate.ToUniversalTime(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UserId = user!.Id
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Событие добавлено!";
            return RedirectToAction(nameof(Events));
        }

        // ── POST /Admin/DeleteEvent/{id} ──────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev != null)
            {
                _db.Events.Remove(ev);
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "Событие удалено.";
            return RedirectToAction(nameof(Events));
        }

        // ══════════════════════════════════════════════════
        // ОБЪЯВЛЕНИЯ ОСИ
        // ══════════════════════════════════════════════════

        // ── GET /Admin/Announcements ──────────────────────
        public async Task<IActionResult> Announcements()
        {
            var list = await _db.Announcements
                .Include(a => a.User)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            ViewData["AdminNav"] = "announcements";
            return View(list);
        }

        // ── POST /Admin/CreateAnnouncement ────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnouncement(string title, string content)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Заполните все поля.";
                return RedirectToAction(nameof(Announcements));
            }

            var user = await _userManager.GetUserAsync(User);
            _db.Announcements.Add(new Announcement
            {
                Title = title.Trim(),
                Content = content.Trim(),
                UserId = user!.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Объявление опубликовано!";
            return RedirectToAction(nameof(Announcements));
        }

        // ── POST /Admin/DeleteAnnouncement/{id} ───────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var ann = await _db.Announcements.FindAsync(id);
            if (ann != null)
            {
                ann.IsActive = false;
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "Объявление удалено.";
            return RedirectToAction(nameof(Announcements));
        }

        // ══════════════════════════════════════════════════
        // ФИНАНСОВЫЕ ОТЧЁТЫ
        // ══════════════════════════════════════════════════

        // ── GET /Admin/FinancialReports ───────────────────
        public async Task<IActionResult> FinancialReports()
        {
            var reports = await _db.FinancialReports
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            ViewData["AdminNav"] = "reports";
            return View(reports);
        }

        // ── POST /Admin/UploadReport ──────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadReport(string title, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Выберите PDF файл.";
                return RedirectToAction(nameof(FinancialReports));
            }

            if (!file.ContentType.Contains("pdf"))
            {
                TempData["Error"] = "Разрешены только PDF файлы.";
                return RedirectToAction(nameof(FinancialReports));
            }

            // Сохраняем файл
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "reports");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}.pdf";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var user = await _userManager.GetUserAsync(User);
            _db.FinancialReports.Add(new FinancialReport
            {
                Title = title.Trim(),
                FileName = fileName,
                OriginalName = file.FileName,
                FileSize = file.Length,
                UserId = user!.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Отчёт загружен!";
            return RedirectToAction(nameof(FinancialReports));
        }

        // ── POST /Admin/DeleteReport/{id} ─────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var report = await _db.FinancialReports.FindAsync(id);
            if (report != null)
            {
                // Удаляем файл с диска
                var filePath = Path.Combine(_env.WebRootPath, "uploads", "reports", report.FileName);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                _db.FinancialReports.Remove(report);
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "Отчёт удалён.";
            return RedirectToAction(nameof(FinancialReports));
        }
    }
}