using korshi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace korshi.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ReportsController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET /Reports
        public async Task<IActionResult> Index()
        {
            var reports = await _db.FinancialReports
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewData["ActiveNav"] = "reports";
            return View(reports);
        }

        // GET /Reports/Download/{id}
        public async Task<IActionResult> Download(int id)
        {
            var report = await _db.FinancialReports.FindAsync(id);
            if (report == null) return NotFound();

            var path = Path.Combine(_env.WebRootPath, "uploads", "reports", report.FileName);
            if (!System.IO.File.Exists(path)) return NotFound();

            var bytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(bytes, "application/pdf", report.OriginalName);
        }
    }
}