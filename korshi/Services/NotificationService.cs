using korshi.Data;
using korshi.Models;
using Microsoft.EntityFrameworkCore;

namespace korshi.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ── Создать уведомление ───────────────────────────
        public async Task CreateAsync(string userId, NotificationType type,
            string message, string? link = null)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = type,
                Message = message,
                Link = link,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        // ── Все уведомления пользователя ──────────────────
        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();
        }

        // ── Количество непрочитанных ──────────────────────
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _db.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        // ── Отметить все как прочитанные ──────────────────
        public async Task MarkAllReadAsync(string userId)
        {
            var unread = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            await _db.SaveChangesAsync();
        }

        // ── Отметить одно как прочитанное ─────────────────
        public async Task MarkReadAsync(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n != null)
            {
                n.IsRead = true;
                await _db.SaveChangesAsync();
            }
        }

        // ── Иконка по типу ────────────────────────────────
        public static string GetIcon(NotificationType type) => type switch
        {
            NotificationType.Like => "bi-heart-fill",
            NotificationType.Comment => "bi-chat-fill",
            NotificationType.FriendRequest => "bi-person-plus-fill",
            NotificationType.ServiceUpdate => "bi-wrench-adjustable",
            NotificationType.NewMessage => "bi-chat-dots-fill",
            NotificationType.AdminVerified => "bi-check-circle-fill",
            _ => "bi-bell-fill"
        };

        // ── Цвет по типу ──────────────────────────────────
        public static string GetColor(NotificationType type) => type switch
        {
            NotificationType.Like => "#EF4444",
            NotificationType.Comment => "#3B82F6",
            NotificationType.FriendRequest => "#8B5CF6",
            NotificationType.ServiceUpdate => "#F59E0B",
            NotificationType.NewMessage => "#10B981",
            NotificationType.AdminVerified => "#10B981",
            _ => "#6B7280"
        };
    }
}