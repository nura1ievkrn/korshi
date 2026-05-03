using static QuestPDF.Helpers.Colors;

namespace korshi.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Автор
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        // Категория
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Навигация
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();

        // Вычисляемые (не в БД)
        public int LikesCount => Likes?.Count ?? 0;
        public int CommentsCount => Comments?.Count ?? 0;
    }
}