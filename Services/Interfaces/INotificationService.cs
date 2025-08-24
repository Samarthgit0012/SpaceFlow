using SpaceFlow.Repositories.Models;
using SpaceFlow.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpaceFlow.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string message);
        Task<PaginatedNotificationsDto> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 10);
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task SendBookingNotificationAsync(string userId, string workspaceName, DateTime startTime, string type);
        Task SendPaymentNotificationAsync(string userId, decimal amount, string bookingDetails);
        Task SendSystemNotificationAsync(string userId, string message);
    }
}