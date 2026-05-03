namespace korshi.Models
{
    public enum NotificationType
    {
        Like = 0,  // Лайкнули пост
        Comment = 1,  // Прокомментировали пост
        FriendRequest = 2, // Запрос в друзья
        ServiceUpdate = 3, // Смена статуса заявки
        NewMessage = 4,  // Новое сообщение
        AdminVerified = 5  // Аккаунт подтверждён
    }

    public class Notification
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Link { get; set; }  // URL для перехода
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Кому уведомление
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}