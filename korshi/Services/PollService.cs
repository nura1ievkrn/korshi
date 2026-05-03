using korshi.Data;
using korshi.Models;
using Microsoft.EntityFrameworkCore;

namespace korshi.Services
{
    public class PollService
    {
        private readonly ApplicationDbContext _db;

        public PollService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ── Все активные голосования ───────────────────────
        public async Task<List<Poll>> GetActivePollsAsync()
        {
            return await _db.Polls
                .Include(p => p.User)
                .Include(p => p.Options)
                    .ThenInclude(o => o.Votes)
                .Include(p => p.Votes)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // ── Один опрос по Id ───────────────────────────────
        public async Task<Poll?> GetPollByIdAsync(int id)
        {
            return await _db.Polls
                .Include(p => p.User)
                .Include(p => p.Options)
                    .ThenInclude(o => o.Votes)
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        // ── Создать голосование ────────────────────────────
        public async Task<Poll> CreatePollAsync(string question,
            List<string> options, DateTime endDate, string userId)
        {
            var poll = new Poll
            {
                Question = question.Trim(),
                EndDate = endDate,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var opt in options.Where(o => !string.IsNullOrWhiteSpace(o)))
            {
                poll.Options.Add(new PollOption { Text = opt.Trim() });
            }

            _db.Polls.Add(poll);
            await _db.SaveChangesAsync();
            return poll;
        }

        // ── Проголосовать ──────────────────────────────────
        public async Task<(bool success, string message)> VoteAsync(
            int pollId, int optionId, string userId)
        {
            var poll = await GetPollByIdAsync(pollId);
            if (poll == null)
                return (false, "Голосование не найдено.");

            if (poll.IsExpired)
                return (false, "Голосование уже завершено.");

            var alreadyVoted = await _db.PollVotes
                .AnyAsync(v => v.PollId == pollId && v.UserId == userId);

            if (alreadyVoted)
                return (false, "Вы уже проголосовали.");

            var option = poll.Options.FirstOrDefault(o => o.Id == optionId);
            if (option == null)
                return (false, "Вариант не найден.");

            _db.PollVotes.Add(new PollVote
            {
                PollId = pollId,
                PollOptionId = optionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return (true, "Голос принят!");
        }

        // ── Проверить голосовал ли пользователь ───────────
        public async Task<int?> GetUserVoteAsync(int pollId, string userId)
        {
            var vote = await _db.PollVotes
                .FirstOrDefaultAsync(v => v.PollId == pollId && v.UserId == userId);
            return vote?.PollOptionId;
        }

        // ── Удалить голосование (только создатель/админ) ──
        public async Task<bool> DeletePollAsync(int id, string userId, bool isAdmin)
        {
            var poll = await _db.Polls.FindAsync(id);
            if (poll == null) return false;
            if (poll.UserId != userId && !isAdmin) return false;

            poll.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Топ-2 активных для виджета в ленте ────────────
        public async Task<List<Poll>> GetTopPollsForWidgetAsync()
        {
            return await _db.Polls
                .Include(p => p.Options)
                    .ThenInclude(o => o.Votes)
                .Include(p => p.Votes)
                .Where(p => p.IsActive && p.EndDate > DateTime.UtcNow)
                .OrderByDescending(p => p.CreatedAt)
                .Take(2)
                .ToListAsync();
        }
    }
}