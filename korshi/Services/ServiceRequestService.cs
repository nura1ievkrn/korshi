using korshi.Data;
using korshi.Models;
using Microsoft.EntityFrameworkCore;

namespace korshi.Services
{
    public class ServiceRequestService
    {
        private readonly ApplicationDbContext _db;

        public ServiceRequestService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ── Все заявки (для админа) ───────────────────────
        public async Task<List<ServiceRequest>> GetAllAsync()
        {
            return await _db.ServiceRequests
                .Include(r => r.User)
                .Include(r => r.Service)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // ── Заявки пользователя ───────────────────────────
        public async Task<List<ServiceRequest>> GetUserRequestsAsync(string userId)
        {
            return await _db.ServiceRequests
                .Include(r => r.Service)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // ── Одна заявка ───────────────────────────────────
        public async Task<ServiceRequest?> GetByIdAsync(int id)
        {
            return await _db.ServiceRequests
                .Include(r => r.User)
                .Include(r => r.Service)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // ── Создать заявку ────────────────────────────────
        public async Task<ServiceRequest> CreateAsync(string title, string description,
            int? serviceId, string userId)
        {
            var request = new ServiceRequest
            {
                Title = title.Trim(),
                Description = description.Trim(),
                ServiceId = serviceId,
                UserId = userId,
                Status = ServiceRequestStatus.New,
                CreatedAt = DateTime.UtcNow
            };

            _db.ServiceRequests.Add(request);
            await _db.SaveChangesAsync();
            return request;
        }

        // ── Изменить статус (только админ) ────────────────
        public async Task<bool> UpdateStatusAsync(int id, ServiceRequestStatus status,
            string? adminNote = null)
        {
            var request = await _db.ServiceRequests.FindAsync(id);
            if (request == null) return false;

            request.Status = status;
            request.UpdatedAt = DateTime.UtcNow;
            if (adminNote != null)
                request.AdminNote = adminNote.Trim();

            await _db.SaveChangesAsync();
            return true;
        }

        // ── Удалить заявку ────────────────────────────────
        public async Task<bool> DeleteAsync(int id, string userId, bool isAdmin)
        {
            var request = await _db.ServiceRequests.FindAsync(id);
            if (request == null) return false;
            if (request.UserId != userId && !isAdmin) return false;

            _db.ServiceRequests.Remove(request);
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Все услуги ────────────────────────────────────
        public async Task<List<Service>> GetServicesAsync()
        {
            return await _db.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
        }

        // ── Статистика для админа ─────────────────────────
        public async Task<(int New, int InProcess, int Done)> GetStatsAsync()
        {
            var newCount = await _db.ServiceRequests.CountAsync(r => r.Status == ServiceRequestStatus.New);
            var inProcessCount = await _db.ServiceRequests.CountAsync(r => r.Status == ServiceRequestStatus.InProcess);
            var doneCount = await _db.ServiceRequests.CountAsync(r => r.Status == ServiceRequestStatus.Done);
            return (newCount, inProcessCount, doneCount);
        }
    }
}