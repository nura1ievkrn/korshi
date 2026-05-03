namespace korshi.Models
{
    // Услуги ЖК — управляются из админки без правки кода
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;  // Сантехника
        public string Description { get; set; } = string.Empty;  // Вызов сантехника
        public string Icon { get; set; } = "bi-wrench";   // bootstrap icon
        public string Color { get; set; } = "#6B7280";     // цвет иконки
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        public ICollection<ServiceRequest> Requests { get; set; } = new List<ServiceRequest>();
    }
}