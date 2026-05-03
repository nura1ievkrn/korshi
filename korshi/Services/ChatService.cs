using korshi.Data;
using korshi.Models;
using Microsoft.EntityFrameworkCore;

namespace korshi.Services
{
    public class ChatService
    {
        private readonly ApplicationDbContext _db;

        public ChatService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ── Все диалоги пользователя ──────────────────────
        public async Task<List<Conversation>> GetConversationsAsync(string userId)
        {
            return await _db.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .OrderByDescending(c => c.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.CreatedAt)
                    .FirstOrDefault())
                .ToListAsync();
        }

        // ── Сообщения диалога ─────────────────────────────
        public async Task<List<DirectMessage>> GetMessagesAsync(int conversationId)
        {
            return await _db.DirectMessages
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        // ── Непрочитанных сообщений ───────────────────────
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _db.DirectMessages
                .Where(m => m.Conversation.User1Id == userId ||
                            m.Conversation.User2Id == userId)
                .Where(m => m.SenderId != userId && !m.IsRead)
                .CountAsync();
        }

        // ── Найти или создать диалог ──────────────────────
        public async Task<Conversation> GetOrCreateConversationAsync(
            string user1Id, string user2Id)
        {
            var conversation = await _db.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.User1Id == user1Id && c.User2Id == user2Id) ||
                    (c.User1Id == user2Id && c.User2Id == user1Id));

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    User1Id = user1Id,
                    User2Id = user2Id,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Conversations.Add(conversation);
                await _db.SaveChangesAsync();
            }

            return conversation;
        }

        // ── Список жильцов для поиска собеседника ────────
        public async Task<List<ApplicationUser>> GetResidentsAsync(string currentUserId)
        {
            return await _db.Users
                .Where(u => u.Id != currentUserId && u.IsVerified && !u.IsBanned)
                .OrderBy(u => u.FirstName)
                .ToListAsync();
        }
    }
}