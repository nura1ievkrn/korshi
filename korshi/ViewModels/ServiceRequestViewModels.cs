using System.ComponentModel.DataAnnotations;
using korshi.Models;

namespace korshi.ViewModels
{
    // ── Список заявок ─────────────────────────────────────
    public class ServiceRequestsIndexViewModel
    {
        public List<ServiceRequest> Requests { get; set; } = new();
        public string Filter { get; set; } = "all";
    }

    // ── Создание заявки ───────────────────────────────────
    public class CreateServiceRequestViewModel
    {
        [Required(ErrorMessage = "Введите заголовок")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "От 5 до 150 символов")]
        [Display(Name = "Заголовок")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Опишите проблему")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "От 10 до 2000 символов")]
        [Display(Name = "Описание проблемы")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Тип услуги")]
        public int? ServiceId { get; set; }

        public List<Service> Services { get; set; } = new();
    }

    // ── Смена статуса (для админа) ────────────────────────
    public class UpdateStatusViewModel
    {
        public int RequestId { get; set; }
        public ServiceRequestStatus Status { get; set; }
        public string? AdminNote { get; set; }
    }
}