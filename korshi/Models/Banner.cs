namespace korshi.Models
{
    public enum BannerType
    {
        Info = 0,  // Синий
        Emergency = 1   // Красный
    }

    public class Banner
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public BannerType Type { get; set; } = BannerType.Info;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
        public string TypeLabel => Type == BannerType.Emergency ? "emergency" : "info";
    }
}