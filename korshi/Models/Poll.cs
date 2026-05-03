namespace korshi.Models
{
    public class Poll
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
        public ICollection<PollVote> Votes { get; set; } = new List<PollVote>();

        public bool IsExpired => DateTime.UtcNow > EndDate;
        public int TotalVotes => Votes?.Count ?? 0;
    }

    public class PollOption
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int PollId { get; set; }
        public Poll Poll { get; set; } = null!;

        public ICollection<PollVote> Votes { get; set; } = new List<PollVote>();

        public int VoteCount => Votes?.Count ?? 0;
    }

    public class PollVote
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public Poll Poll { get; set; } = null!;
        public int PollOptionId { get; set; }
        public PollOption Option { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}