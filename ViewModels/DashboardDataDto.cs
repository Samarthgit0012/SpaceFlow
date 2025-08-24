using SpaceFlow.Repositories.Models;
using System.Collections.Generic;

namespace SpaceFlow.ViewModels
{
    public class DashboardDataDto
    {
        public int TotalWorkspaces { get; set; }
        public int AvailableWorkspaces { get; set; }
        public int TotalBookings { get; set; }
        public int ActiveUsers { get; set; }
        public List<WorkspaceTypeStatsDto> PopularTypes { get; set; } = new List<WorkspaceTypeStatsDto>();
        public List<Workspace> FeaturedWorkspaces { get; set; } = new List<Workspace>();
        public ApplicationStatsDto ApplicationStats { get; set; } = new ApplicationStatsDto();
        public List<ActivityDto> RecentActivities { get; set; } = new List<ActivityDto>();
    }
}
