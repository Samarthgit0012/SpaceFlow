using SpaceFlow.Repositories.Models;
using SpaceFlow.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpaceFlow.Services.Interfaces
{
    public interface IHomeService
    {
        Task<DashboardDataDto> GetDashboardDataAsync(string? userId = null);
        Task<List<Workspace>> GetFeaturedWorkspacesAsync(int count = 6);
        Task<ApplicationStatsDto> GetApplicationStatsAsync();
        Task<List<ActivityDto>> GetRecentActivitiesAsync(string? userId = null, int count = 10);
        Task<WorkspaceSearchResultDto> SearchWorkspacesAsync(WorkspaceSearchDto searchDto);
        Task<List<WorkspaceAvailabilityDto>> GetWorkspaceAvailabilityAsync(int workspaceId, DateTime date);
        Task<bool> SubscribeToNewsletterAsync(NewsletterSubscriptionDto subscriptionDto);
        Task<bool> SendContactMessageAsync(ContactFormDto contactDto);
        Task<List<Workspace>> GetWorkspaceRecommendationsAsync(string userId, int count = 5);
        
        // Additional methods needed by controller
        Task<List<WorkspaceTypeStatsDto>> GetPopularWorkspaceTypesAsync();
        Task<List<Booking>> GetUpcomingBookingsAsync(string userId, int count = 5);
        Task<List<WorkspaceAvailabilityDto>> GetWorkspaceAvailabilityAsync(DateTime date);
        Task<PaginatedNotificationsDto> GetUserNotificationsAsync(string userId, int page, int pageSize);
        Task<bool> MarkNotificationAsReadAsync(int notificationId, string userId);
        Task<bool> ProcessContactFormAsync(ContactFormDto contactDto);
    }
}
