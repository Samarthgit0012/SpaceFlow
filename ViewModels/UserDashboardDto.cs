using SpaceFlow.Repositories.Models;

namespace SpaceFlow.ViewModels
{
    public class UserDashboardDto
    {
        public string WelcomeMessage { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int UpcomingBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalSpent { get; set; }
        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
        public List<Workspace> RecommendedWorkspaces { get; set; } = new List<Workspace>();
        public List<NotificationDto> UnreadNotifications { get; set; } = new List<NotificationDto>();
    }
}
