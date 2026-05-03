namespace korshi.Models
{
    public class Conversation
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Два участника
        public string User1Id { get; set; } = string.Empty;
        public ApplicationUser User1 { get; set; } = null!;

        public string User2Id { get; set; } = string.Empty;
        public ApplicationUser User2 { get; set; } = null!;

        public ICollection<DirectMessage> Messages { get; set; } = new List<DirectMessage>();

        public DirectMessage? LastMessage => Messages?
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefault();
    }

    public class DirectMessage
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser Sender { get; set; } = null!;
    }
}