using SpaceFlow.Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpaceFlow.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string userId, int page = 1, int pageSize = 10);
        Task<int> GetUnreadCountAsync(string userId);
        Task<Notification?> GetNotificationByIdAsync(int id);
        Task UpdateNotificationAsync(Notification notification);
        Task DeleteNotificationAsync(Notification notification);
        Task MarkAllAsReadAsync(string userId);
    }
}