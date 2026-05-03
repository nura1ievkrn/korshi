using System.ComponentModel.DataAnnotations;
using korshi.Models;

namespace korshi.ViewModels
{
    // ── Главная лента ─────────────────────────────────────
    public class FeedViewModel
    {
        public List<Post> Posts { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public HashSet<int> LikedPostIds { get; set; } = new();
        public string ActiveSlug { get; set; } = "all";
        public List<Poll> TopPolls { get; set; } = new();
    }

    // ── Создание поста ────────────────────────────────────
    public class CreatePostViewModel
    {
        [Required(ErrorMessage = "Введите заголовок")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "От 3 до 150 символов")]
        [Display(Name = "Заголовок")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите текст")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "От 10 до 5000 символов")]
        [Display(Name = "Текст")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите категорию")]
        [Display(Name = "Категория")]
        public int CategoryId { get; set; }

        // Для дропдауна категорий
        public List<Category> Categories { get; set; } = new();
    }

    // ── Детальная страница поста ──────────────────────────
    public class PostDetailViewModel
    {
        public Post Post { get; set; } = null!;
        public bool IsLiked { get; set; }
        public bool IsOwner { get; set; }
        public bool IsAdmin { get; set; }
        public CommentViewModel NewComment { get; set; } = new();
    }

    // ── Комментарий ───────────────────────────────────────
    public class CommentViewModel
    {
        public int PostId { get; set; }

        [Required(ErrorMessage = "Введите текст комментария")]
        [StringLength(1000, MinimumLength = 1)]
        [Display(Name = "Комментарий")]
        public string Content { get; set; } = string.Empty;
    }
}