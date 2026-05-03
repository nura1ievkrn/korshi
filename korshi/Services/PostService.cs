using korshi.Data;
using korshi.Models;
using Microsoft.EntityFrameworkCore;

namespace korshi.Services
{
    public class PostService
    {
        private readonly ApplicationDbContext _db;
        private readonly NotificationService _notifService;

        public PostService(ApplicationDbContext db, NotificationService notifService)
        {
            _db = db;
            _notifService = notifService;
        }

        // ── Получить все посты (с фильтром по категории) ──
        public async Task<List<Post>> GetPostsAsync(string? slug = null)
        {
            var query = _db.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(slug))
                query = query.Where(p => p.Category.Slug == slug);

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // ── Получить один пост ────────────────────────────
        public async Task<Post?> GetPostByIdAsync(int id)
        {
            return await _db.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        // ── Создать пост ──────────────────────────────────
        public async Task<Post> CreatePostAsync(string title, string content,
            int categoryId, string userId)
        {
            var post = new Post
            {
                Title = title.Trim(),
                Content = content.Trim(),
                CategoryId = categoryId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();
            return post;
        }

        // ── Удалить пост (только автор или админ) ────────
        public async Task<bool> DeletePostAsync(int id, string userId, bool isAdmin)
        {
            var post = await _db.Posts.FindAsync(id);
            if (post == null) return false;
            if (post.UserId != userId && !isAdmin) return false;

            post.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Лайк / снять лайк ────────────────────────────
        public async Task<(bool liked, int count)> ToggleLikeAsync(int postId, string userId)
        {
            var existing = await _db.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existing != null)
            {
                _db.Likes.Remove(existing);
                await _db.SaveChangesAsync();
                var count = await _db.Likes.CountAsync(l => l.PostId == postId);
                return (false, count);
            }
            else
            {
                _db.Likes.Add(new Like
                {
                    PostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();

                // Уведомление автору поста
                var post = await _db.Posts.FindAsync(postId);
                if (post != null && post.UserId != userId)
                {
                    var liker = await _db.Users.FindAsync(userId);
                    await _notifService.CreateAsync(
                        post.UserId,
                        NotificationType.Like,
                        $"{liker?.FullName ?? "Сосед"} лайкнул ваш пост «{post.Title}»",
                        $"/Feed/Detail/{postId}");
                }

                var count2 = await _db.Likes.CountAsync(l => l.PostId == postId);
                return (true, count2);
            }
        }

        // ── Проверить лайкнул ли пользователь пост ───────
        public async Task<bool> IsLikedByUserAsync(int postId, string userId)
        {
            return await _db.Likes
                .AnyAsync(l => l.PostId == postId && l.UserId == userId);
        }

        // ── Добавить комментарий ──────────────────────────
        public async Task<Comment> AddCommentAsync(int postId, string content, string userId)
        {
            var comment = new Comment
            {
                PostId = postId,
                Content = content.Trim(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            // Уведомление автору поста
            var post = await _db.Posts.FindAsync(postId);
            if (post != null && post.UserId != userId)
            {
                var commenter = await _db.Users.FindAsync(userId);
                await _notifService.CreateAsync(
                    post.UserId,
                    NotificationType.Comment,
                    $"{commenter?.FullName ?? "Сосед"} прокомментировал ваш пост «{post.Title}»",
                    $"/Feed/Detail/{postId}#comments");
            }

            return comment;
        }

        // ── Удалить комментарий ───────────────────────────
        public async Task<bool> DeleteCommentAsync(int id, string userId, bool isAdmin)
        {
            var comment = await _db.Comments.FindAsync(id);
            if (comment == null) return false;
            if (comment.UserId != userId && !isAdmin) return false;

            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Получить все категории ────────────────────────
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _db.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
        }

        // ── Получить активный баннер ──────────────────────
        public async Task<Banner?> GetActiveBannerAsync()
        {
            return await _db.Banners
                .Where(b => b.IsActive &&
                       (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow))
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // ── Посты пользователя (для профиля) ─────────────
        public async Task<List<Post>> GetUserPostsAsync(string userId)
        {
            return await _db.Posts
                .Include(p => p.Category)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.UserId == userId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // ── Лайкнутые посты пользователя (для профиля) ───
        public async Task<List<Post>> GetLikedPostsAsync(string userId)
        {
            return await _db.Likes
                .Include(l => l.Post)
                    .ThenInclude(p => p.Category)
                .Include(l => l.Post)
                    .ThenInclude(p => p.Likes)
                .Include(l => l.Post)
                    .ThenInclude(p => p.Comments)
                .Where(l => l.UserId == userId && l.Post.IsActive)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => l.Post)
                .ToListAsync();
        }
    }
}