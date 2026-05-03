using korshi.Models;
using korshi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace korshi.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ChatService _chatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(
            ChatService chatService,
            UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _userManager = userManager;
        }

        // ── GET /Chat ─────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var conversations = await _chatService.GetConversationsAsync(user.Id);
            var residents = await _chatService.GetResidentsAsync(user.Id);

            ViewData["ActiveNav"] = "chat";
            ViewBag.CurrentUser = user;
            ViewBag.Conversations = conversations;
            ViewBag.Residents = residents;

            return View();
        }

        // ── GET /Chat/Open/{userId} ───────────────────────
        public async Task<IActionResult> Open(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var otherUser = await _userManager.FindByIdAsync(userId);
            if (otherUser == null) return NotFound();

            var conversation = await _chatService
                .GetOrCreateConversationAsync(currentUser.Id, userId);

            var messages = await _chatService.GetMessagesAsync(conversation.Id);

            ViewData["ActiveNav"] = "chat";
            ViewBag.CurrentUser = currentUser;
            ViewBag.OtherUser = otherUser;
            ViewBag.Conversation = conversation;
            ViewBag.Messages = messages;
            ViewBag.Conversations = await _chatService.GetConversationsAsync(currentUser.Id);
            ViewBag.Residents = await _chatService.GetResidentsAsync(currentUser.Id);

            return View("Index");
        }
    }
}