using System.ComponentModel.DataAnnotations;
using korshi.Models;

namespace korshi.ViewModels
{
    // ── Список голосований ────────────────────────────────
    public class PollsIndexViewModel
    {
        public List<PollCardViewModel> Polls { get; set; } = new();
    }

    // ── Карточка голосования ──────────────────────────────
    public class PollCardViewModel
    {
        public Poll Poll { get; set; } = null!;
        public int? UserVoteId { get; set; }
        public bool HasVoted => UserVoteId.HasValue;
        public bool CanVote => !HasVoted && !Poll.IsExpired;
    }

    // ── Создание голосования ──────────────────────────────
    public class CreatePollViewModel
    {
        [Required(ErrorMessage = "Введите вопрос")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "От 5 до 300 символов")]
        [Display(Name = "Вопрос")]
        public string Question { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите дату окончания")]
        [Display(Name = "Дата окончания")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7);

        [Required(ErrorMessage = "Введите вариант 1")]
        [Display(Name = "Вариант 1")]
        public string Option1 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите вариант 2")]
        [Display(Name = "Вариант 2")]
        public string Option2 { get; set; } = string.Empty;

        [Display(Name = "Вариант 3 (необязательно)")]
        public string? Option3 { get; set; }

        [Display(Name = "Вариант 4 (необязательно)")]
        public string? Option4 { get; set; }

        [Display(Name = "Вариант 5 (необязательно)")]
        public string? Option5 { get; set; }

        public List<string> GetOptions() => new[]
            { Option1, Option2, Option3, Option4, Option5 }
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Select(o => o!)
            .ToList();
    }
}