using korshi.Models;

namespace korshi.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int PendingUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int BannedUsers { get; set; }
        public int TotalPosts { get; set; }
        public int OpenRequests { get; set; }
        public int ActivePolls { get; set; }

        public List<ApplicationUser> PendingList { get; set; } = new();
    }
}