namespace korshi.Models
{
    public enum FriendshipStatus
    {
        Pending = 0,  // Запрос отправлен
        Accepted = 1,  // Принят
        Declined = 2   // Отклонён
    }

    public class Friendship
    {
        public int Id { get; set; }
        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Кто отправил запрос
        public string RequesterId { get; set; } = string.Empty;
        public ApplicationUser Requester { get; set; } = null!;

        // Кому отправили
        public string AddresseeId { get; set; } = string.Empty;
        public ApplicationUser Addressee { get; set; } = null!;
    }
}