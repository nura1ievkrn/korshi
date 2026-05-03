using korshi.Data;
using korshi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace korshi.Controllers
{
    [Authorize]
    public class AnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AnnouncementsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /Announcements
        public async Task<IActionResult> Index()
        {
            var announcements = await _db.Announcements
                .Include(a => a.User)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewData["ActiveNav"] = "announcements";
            return View(announcements);
        }
    }
}