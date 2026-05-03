namespace korshi.Models
{
    public enum ServiceRequestStatus
    {
        New = 0,  // Новая
        InProcess = 1,  // В работе
        Done = 2   // Выполнена
    }

    public class ServiceRequest
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.New;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? AdminNote { get; set; }

        // Тип заявки (сантехника, электрика и т.д.) — берётся из Service
        public int? ServiceId { get; set; }
        public Service? Service { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public string StatusLabel => Status switch
        {
            ServiceRequestStatus.New => "Новая",
            ServiceRequestStatus.InProcess => "В работе",
            ServiceRequestStatus.Done => "Выполнена",
            _ => "Неизвестно"
        };

        public string StatusClass => Status switch
        {
            ServiceRequestStatus.New => "status-new",
            ServiceRequestStatus.InProcess => "status-process",
            ServiceRequestStatus.Done => "status-done",
            _ => ""
        };
    }
}