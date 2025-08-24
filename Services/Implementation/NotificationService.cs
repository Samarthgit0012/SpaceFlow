using SpaceFlow.Repositories.Interfaces;
using SpaceFlow.Repositories.Models;
using SpaceFlow.Services.Interfaces;
using SpaceFlow.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceFlow.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(INotificationRepository notificationRepository, ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task CreateNotificationAsync(string userId, string message)
        {
            try
            {
                var notification = new Notification
                {
                    ApplicationUserId = userId,
                    Message = message,
                    IsRead = false,
                    DateCreated = DateTime.UtcNow
                };

                await _notificationRepository.CreateNotificationAsync(notification);
                _logger.LogInformation("Notification created for user {UserId}: {Message}", userId, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", userId);
            }
        }

        public async Task<PaginatedNotificationsDto> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId, page, pageSize);
                var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);

                // For total count, we'd need to add this to the repository
                // For now, we'll estimate based on current page
                var totalCount = notifications.Count() + ((page - 1) * pageSize) + (notifications.Count() == pageSize ? pageSize : 0);
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var notificationDtos = notifications.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    DateCreated = n.DateCreated,
                    Type = DetermineNotificationType(n.Message)
                }).ToList();

                return new PaginatedNotificationsDto
                {
                    Notifications = notificationDtos,
                    TotalCount = totalCount,
                    UnreadCount = unreadCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                return new PaginatedNotificationsDto();
            }
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            try
            {
                var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId);

                if (notification == null || notification.ApplicationUserId != userId)
                {
                    return false;
                }

                notification.IsRead = true;
                await _notificationRepository.UpdateNotificationAsync(notification);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
                return false;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            try
            {
                await _notificationRepository.MarkAllAsReadAsync(userId);
                _logger.LogInformation("All notifications marked as read for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                return false;
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            try
            {
                return await _notificationRepository.GetUnreadCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                return 0;
            }
        }

        public async Task SendBookingNotificationAsync(string userId, string workspaceName, DateTime startTime, string type)
        {
            string message = type.ToLower() switch
            {
                "created" => $"Your booking for {workspaceName} on {startTime:MMM dd, yyyy} at {startTime:h:mm tt} has been created successfully.",
                "confirmed" => $"Your booking for {workspaceName} on {startTime:MMM dd, yyyy} at {startTime:h:mm tt} has been confirmed.",
                "cancelled" => $"Your booking for {workspaceName} on {startTime:MMM dd, yyyy} at {startTime:h:mm tt} has been cancelled.",
                "reminder" => $"Reminder: Your booking for {workspaceName} starts in 1 hour ({startTime:h:mm tt}).",
                _ => $"Update regarding your booking for {workspaceName} on {startTime:MMM dd, yyyy}."
            };

            await CreateNotificationAsync(userId, message);
        }

        public async Task SendPaymentNotificationAsync(string userId, decimal amount, string bookingDetails)
        {
            var message = $"Payment of ₹{amount:F2} received for {bookingDetails}. Thank you!";
            await CreateNotificationAsync(userId, message);
        }

        public async Task SendSystemNotificationAsync(string userId, string message)
        {
            await CreateNotificationAsync(userId, message);
        }

        private string DetermineNotificationType(string message)
        {
            if (message.Contains("confirmed") || message.Contains("successful"))
                return "success";

            if (message.Contains("cancelled") || message.Contains("failed"))
                return "error";

            if (message.Contains("reminder") || message.Contains("upcoming"))
                return "warning";

            return "info";
        }
    }
}