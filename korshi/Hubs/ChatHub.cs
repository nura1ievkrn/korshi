using korshi.Data;
using korshi.Models;
using korshi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace korshi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notifService;

        // Онлайн пользователи: ConnectionId → UserId
        private static readonly Dictionary<string, string> _onlineUsers = new();

        public ChatHub(ApplicationDbContext db,
           UserManager<ApplicationUser> userManager,
           NotificationService notifService)
        {
            _db = db;
            _userManager = userManager;
            _notifService = notifService;
        }

        // ── Подключение ───────────────────────────────────
        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user != null)
            {
                lock (_onlineUsers)
                    _onlineUsers[Context.ConnectionId] = user.Id;

                // Уведомить всех об онлайн статусе
                await Clients.All.SendAsync("UserOnline", user.Id, user.FullName);
                await Clients.All.SendAsync("OnlineCount", _onlineUsers.Values.Distinct().Count());
            }
            await base.OnConnectedAsync();
        }

        // ── Отключение ────────────────────────────────────
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_onlineUsers.TryGetValue(Context.ConnectionId, out var userId))
            {
                lock (_onlineUsers)
                    _onlineUsers.Remove(Context.ConnectionId);

                await Clients.All.SendAsync("UserOffline", userId);
                await Clients.All.SendAsync("OnlineCount", _onlineUsers.Values.Distinct().Count());
            }
            await base.OnDisconnectedAsync(exception);
        }

        // ── Отправить сообщение ───────────────────────────
        public async Task SendMessage(string receiverId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            var sender = await _userManager.GetUserAsync(Context.User!);
            if (sender == null) return;

            // Найти или создать диалог
            var conversation = await _db.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.User1Id == sender.Id && c.User2Id == receiverId) ||
                    (c.User1Id == receiverId && c.User2Id == sender.Id));

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    User1Id = sender.Id,
                    User2Id = receiverId,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Conversations.Add(conversation);
                await _db.SaveChangesAsync();
            }

            // Сохранить сообщение
            var message = new DirectMessage
            {
                ConversationId = conversation.Id,
                SenderId = sender.Id,
                Content = content.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _db.DirectMessages.Add(message);
            await _db.SaveChangesAsync();

            // Уведомление получателю
            var receiver = await _userManager.FindByIdAsync(receiverId);
            if (receiver != null)
            {
                await _notifService.CreateAsync(
                    receiverId,
                    NotificationType.NewMessage,
                    $"{sender.FullName} отправил вам сообщение",
                    $"/Chat/Open/{sender.Id}");
            }

            // Отправить сообщение получателю и отправителю
            var messageData = new
            {
                id = message.Id,
                senderId = sender.Id,
                senderName = sender.FullName,
                senderInitials = sender.Initials,
                content = message.Content,
                createdAt = message.CreatedAt.ToLocalTime().ToString("HH:mm"),
                conversationId = conversation.Id
            };

            // Найти connection получателя
            var receiverConnections = _onlineUsers
                .Where(x => x.Value == receiverId)
                .Select(x => x.Key)
                .ToList();

            foreach (var conn in receiverConnections)
                await Clients.Client(conn).SendAsync("ReceiveMessage", messageData);

            // Отправить самому себе (подтверждение)
            await Clients.Caller.SendAsync("ReceiveMessage", messageData);
        }

        // ── Отметить как прочитанное ──────────────────────
        public async Task MarkAsRead(int conversationId)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user == null) return;

            var messages = await _db.DirectMessages
                .Where(m => m.ConversationId == conversationId &&
                            m.SenderId != user.Id &&
                            !m.IsRead)
                .ToListAsync();

            foreach (var msg in messages)
                msg.IsRead = true;

            await _db.SaveChangesAsync();
        }
    }
}