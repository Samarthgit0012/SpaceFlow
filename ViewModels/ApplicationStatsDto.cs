using System.Collections.Generic;

namespace SpaceFlow.ViewModels
{
    public class ApplicationStatsDto
    {
        public int TotalWorkspaces { get; set; }
        public int TotalUsers { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int BookingsToday { get; set; }
        public int BookingsThisWeek { get; set; }
        public int BookingsThisMonth { get; set; }
        public double AverageBookingDuration { get; set; }
        public double AverageBookingsPerDay { get; set; }
        public List<WorkspaceTypeStatsDto> WorkspaceTypeStats { get; set; } = new List<WorkspaceTypeStatsDto>();
        public List<WorkspaceTypeStatsDto> PopularWorkspaceTypes { get; set; } = new List<WorkspaceTypeStatsDto>();
    }
}
