using korshi.Models;
using korshi.Services;
using korshi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace korshi.Controllers
{
    [Authorize]
    public class FeedController : Controller
    {
        private readonly PostService _postService;
        private readonly PollService _pollService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly korshi.Data.ApplicationDbContext _db;

        public FeedController(PostService postService,
            PollService pollService,
            UserManager<ApplicationUser> userManager,
            korshi.Data.ApplicationDbContext db)
        {
            _postService = postService;
            _pollService = pollService;
            _userManager = userManager;
            _db = db;
        }

        // ── GET /Feed  или  GET / ─────────────────────────
        public async Task<IActionResult> Index(string? category = null)
        {
            var posts = await _postService.GetPostsAsync(category);
            var categories = await _postService.GetCategoriesAsync();
            var banner = await _postService.GetActiveBannerAsync();
            var user = await _userManager.GetUserAsync(User);
            var topPolls = await _pollService.GetTopPollsForWidgetAsync();

            // Предстоящие события
            var upcomingEvents = await _db.Events
                .Where(e => e.IsActive && e.EventDate > DateTime.UtcNow)
                .OrderBy(e => e.EventDate)
                .Take(3)
                .ToListAsync();
            ViewBag.UpcomingEvents = upcomingEvents;

            // Лайкнутые посты текущего пользователя
            var likedIds = user != null
                ? (await _postService.GetLikedPostsAsync(user.Id))
                          .Select(p => p.Id).ToHashSet()
                : new HashSet<int>();

            if (banner != null)
            {
                ViewData["BannerMessage"] = banner.Message;
                ViewData["BannerType"] = banner.TypeLabel;
            }

            ViewData["ActiveNav"] = "wall";
            ViewData["ActiveCategory"] = category ?? "all";

            var vm = new FeedViewModel
            {
                Posts = posts,
                Categories = categories,
                LikedPostIds = likedIds,
                ActiveSlug = category ?? "all",
                TopPolls = topPolls
            };

            return View(vm);
        }

        // ── GET /Feed/Create ──────────────────────────────
        public async Task<IActionResult> Create()
        {
            var categories = await _postService.GetCategoriesAsync();
            ViewData["ActiveNav"] = "wall";
            return View(new CreatePostViewModel { Categories = categories });
        }

        // ── POST /Feed/Create ─────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _postService.GetCategoriesAsync();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _postService.CreatePostAsync(
                model.Title, model.Content, model.CategoryId, user.Id);

            TempData["Success"] = "Пост успешно опубликован!";
            return RedirectToAction(nameof(Index));
        }

        // ── GET /Feed/Detail/{id} ─────────────────────────
        public async Task<IActionResult> Detail(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isLiked = user != null &&
                           await _postService.IsLikedByUserAsync(id, user.Id);
            var isAdmin = user != null &&
                           await _userManager.IsInRoleAsync(user, "AdminOSI");

            ViewData["ActiveNav"] = "wall";

            var vm = new PostDetailViewModel
            {
                Post = post,
                IsLiked = isLiked,
                IsOwner = user?.Id == post.UserId,
                IsAdmin = isAdmin,
                NewComment = new CommentViewModel { PostId = id }
            };

            return View(vm);
        }

        // ── POST /Feed/Like/{id} ──────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id, string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _postService.ToggleLikeAsync(id, user.Id);

            // Вернуться туда откуда пришли
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // ── POST /Feed/Comment ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(CommentViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Content))
                return RedirectToAction(nameof(Detail), new { id = model.PostId });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _postService.AddCommentAsync(model.PostId, model.Content, user.Id);

            return RedirectToAction(nameof(Detail), new { id = model.PostId });
        }

        // ── POST /Feed/DeleteComment/{id} ─────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id, int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var isAdmin = await _userManager.IsInRoleAsync(user, "AdminOSI");
            await _postService.DeleteCommentAsync(id, user.Id, isAdmin);

            return RedirectToAction(nameof(Detail), new { id = postId });
        }

        // ── POST /Feed/Delete/{id} ────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var isAdmin = await _userManager.IsInRoleAsync(user, "AdminOSI");
            await _postService.DeletePostAsync(id, user.Id, isAdmin);

            TempData["Success"] = "Пост удалён.";
            return RedirectToAction(nameof(Index));
        }
    }
}