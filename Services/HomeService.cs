using Microsoft.EntityFrameworkCore;
using SpaceFlow.Data;
using SpaceFlow.Repositories.Models;
using SpaceFlow.Services.Interfaces;
using SpaceFlow.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceFlow.Services.Implementation
{
    public class HomeService : IHomeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<HomeService> _logger;

        public HomeService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<HomeService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<DashboardDataDto> GetDashboardDataAsync(string? userId = null)
        {
            try
            {
                var featuredWorkspaces = await GetFeaturedWorkspacesAsync(6);
                var stats = await GetApplicationStatsAsync();
                var activities = await GetRecentActivitiesAsync(userId, 10);

                return new DashboardDataDto
                {
                    FeaturedWorkspaces = featuredWorkspaces,
                    ApplicationStats = stats,
                    RecentActivities = activities
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data for user {UserId}", userId);
                return new DashboardDataDto();
            }
        }

        public async Task<List<Workspace>> GetFeaturedWorkspacesAsync(int count = 6)
        {
            try
            {
                return await _context.Workspaces
                    .Where(w => w.IsAvailable)
                    .Include(w => w.Bookings)
                    .OrderByDescending(w => w.Bookings.Count)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured workspaces");
                return new List<Workspace>();
            }
        }

        public async Task<ApplicationStatsDto> GetApplicationStatsAsync()
        {
            try
            {
                var totalWorkspaces = await _context.Workspaces.CountAsync();
                var totalBookings = await _context.Bookings.CountAsync();
                var totalUsers = await _context.Users.CountAsync();
                var totalRevenue = await _context.Payments.SumAsync(p => p.Amount);

                // Get booking trends (last 30 days)
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var recentBookings = await _context.Bookings
                    .Where(b => b.StartTime >= thirtyDaysAgo)
                    .ToListAsync();

                var avgBookingsPerDay = recentBookings.Count > 0 ? recentBookings.Count / 30.0 : 0;

                // Calculate average booking duration
                var avgDuration = recentBookings.Count > 0
                    ? recentBookings.Average(b => (b.EndTime - b.StartTime).TotalHours)
                    : 0.0;

                return new ApplicationStatsDto
                {
                    TotalWorkspaces = totalWorkspaces,
                    TotalBookings = totalBookings,
                    TotalUsers = totalUsers,
                    TotalRevenue = totalRevenue,
                    AverageBookingsPerDay = Math.Round(avgBookingsPerDay, 2),
                    AverageBookingDuration = Math.Round(avgDuration, 2),
                    PopularWorkspaceTypes = await GetWorkspaceTypeStatsAsync()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application stats");
                return new ApplicationStatsDto();
            }
        }

        private async Task<List<WorkspaceTypeStatsDto>> GetWorkspaceTypeStatsAsync()
        {
            try
            {
                return await _context.Workspaces
                    .GroupBy(w => w.Type)
                    .Select(g => new WorkspaceTypeStatsDto
                    {
                        Type = g.Key,
                        Count = g.Count(),
                        BookingsCount = g.SelectMany(w => w.Bookings).Count(),
                        Revenue = g.SelectMany(w => w.Bookings).Sum(b => b.TotalPrice)
                    })
                    .OrderByDescending(s => s.BookingsCount)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspace type stats");
                return new List<WorkspaceTypeStatsDto>();
            }
        }

        public async Task<List<ActivityDto>> GetRecentActivitiesAsync(string? userId = null, int count = 10)
        {
            try
            {
                var query = _context.Bookings.AsQueryable();

                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(b => b.ApplicationUserId == userId);
                }

                var recentBookings = await query
                    .Include(b => b.Workspace)
                    .Include(b => b.ApplicationUser)
                    .OrderByDescending(b => b.StartTime)
                    .Take(count)
                    .ToListAsync();

                return recentBookings.Select(b => new ActivityDto
                {
                    Id = b.Id,
                    Type = "Booking",
                    Description = $"Booked {b.Workspace.Name}",
                    UserName = b.ApplicationUser.FullName ?? "Unknown User",
                    Timestamp = b.StartTime,
                    Status = b.Status.ToString()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities for user {UserId}", userId);
                return new List<ActivityDto>();
            }
        }

        public async Task<WorkspaceSearchResultDto> SearchWorkspacesAsync(WorkspaceSearchDto searchDto)
        {
            try
            {
                var query = _context.Workspaces.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    query = query.Where(w =>
                        w.Name.Contains(searchDto.SearchTerm) ||
                        w.Type.Contains(searchDto.SearchTerm) ||
                        (w.Amenities != null && w.Amenities.Contains(searchDto.SearchTerm)));
                }

                if (!string.IsNullOrEmpty(searchDto.Type))
                {
                    query = query.Where(w => w.Type == searchDto.Type);
                }

                if (searchDto.MinCapacity.HasValue)
                {
                    query = query.Where(w => w.Capacity >= searchDto.MinCapacity.Value);
                }

                if (searchDto.MaxCapacity.HasValue)
                {
                    query = query.Where(w => w.Capacity <= searchDto.MaxCapacity.Value);
                }

                if (searchDto.MinPrice.HasValue)
                {
                    query = query.Where(w => w.PricePerHour >= searchDto.MinPrice.Value);
                }

                if (searchDto.MaxPrice.HasValue)
                {
                    query = query.Where(w => w.PricePerHour <= searchDto.MaxPrice.Value);
                }

                if (searchDto.IsAvailable.HasValue)
                {
                    query = query.Where(w => w.IsAvailable == searchDto.IsAvailable.Value);
                }

                // Apply sorting
                query = searchDto.SortBy?.ToLower() switch
                {
                    "name" => searchDto.SortDescending ? query.OrderByDescending(w => w.Name) : query.OrderBy(w => w.Name),
                    "price" => searchDto.SortDescending ? query.OrderByDescending(w => w.PricePerHour) : query.OrderBy(w => w.PricePerHour),
                    "capacity" => searchDto.SortDescending ? query.OrderByDescending(w => w.Capacity) : query.OrderBy(w => w.Capacity),
                    "created" => searchDto.SortDescending ? query.OrderByDescending(w => w.CreatedDate) : query.OrderBy(w => w.CreatedDate),
                    _ => query.OrderBy(w => w.Name)
                };

                var totalCount = await query.CountAsync();

                var workspaces = await query
                    .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return new WorkspaceSearchResultDto
                {
                    Workspaces = workspaces,
                    TotalCount = totalCount,
                    PageNumber = searchDto.PageNumber,
                    PageSize = searchDto.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching workspaces with term: {SearchTerm}", searchDto.SearchTerm);
                return new WorkspaceSearchResultDto();
            }
        }

        public async Task<List<WorkspaceAvailabilityDto>> GetWorkspaceAvailabilityAsync(int workspaceId, DateTime date)
        {
            try
            {
                var workspace = await _context.Workspaces.FindAsync(workspaceId);
                if (workspace == null)
                {
                    return new List<WorkspaceAvailabilityDto>();
                }

                var startOfDay = date.Date;
                var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

                var bookings = await _context.Bookings
                    .Where(b => b.WorkspaceId == workspaceId &&
                                b.StartTime <= endOfDay &&
                                b.EndTime >= startOfDay &&
                                b.Status != BookingStatus.Canceled)
                    .ToListAsync();

                var availabilitySlots = new List<WorkspaceAvailabilityDto>();

                // Generate hourly slots for the day (8 AM to 8 PM)
                for (int hour = 8; hour < 20; hour++)
                {
                    var slotStart = startOfDay.AddHours(hour);
                    var slotEnd = slotStart.AddHours(1);

                    var isAvailable = !bookings.Any(b =>
                        b.StartTime < slotEnd && b.EndTime > slotStart);

                    availabilitySlots.Add(new WorkspaceAvailabilityDto
                    {
                        WorkspaceId = workspaceId,
                        Date = date,
                        TimeSlots = new List<TimeSlotDto>
                        {
                            new TimeSlotDto
                            {
                                StartTime = slotStart.TimeOfDay,
                                EndTime = slotEnd.TimeOfDay,
                                IsAvailable = isAvailable
                            }
                        }
                    });
                }

                return availabilitySlots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspace availability for workspace {WorkspaceId} on {Date}", workspaceId, date);
                return new List<WorkspaceAvailabilityDto>();
            }
        }

        public async Task<bool> SubscribeToNewsletterAsync(NewsletterSubscriptionDto subscriptionDto)
        {
            try
            {
                // Here you would typically save to a newsletter subscription table
                // For now, we'll just send a welcome email
                await _emailService.SendWelcomeEmailAsync(subscriptionDto.Email, subscriptionDto.Name);

                _logger.LogInformation("Newsletter subscription for {Email}", subscriptionDto.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing {Email} to newsletter", subscriptionDto.Email);
                return false;
            }
        }

        public async Task<bool> SendContactMessageAsync(ContactFormDto contactDto)
        {
            try
            {
                await _emailService.SendContactMessageAsync(contactDto);
                _logger.LogInformation("Contact message sent from {Email}", contactDto.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending contact message from {Email}", contactDto.Email);
                return false;
            }
        }

        public async Task<List<Workspace>> GetWorkspaceRecommendationsAsync(string userId, int count = 5)
        {
            try
            {
                // Get user's booking history to understand preferences
                var userBookings = await _context.Bookings
                    .Where(b => b.ApplicationUserId == userId)
                    .Include(b => b.Workspace)
                    .ToListAsync();

                if (!userBookings.Any())
                {
                    // If no history, return popular workspaces
                    return await _context.Workspaces
                        .Where(w => w.IsAvailable)
                        .Include(w => w.Bookings)
                        .OrderByDescending(w => w.Bookings.Count())
                        .Take(count)
                        .ToListAsync();
                }

                // Analyze user preferences
                var preferredTypes = userBookings
                    .GroupBy(b => b.Workspace.Type)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .Take(3)
                    .ToList();

                var avgCapacityNeeded = userBookings.Average(b => b.Workspace.Capacity);

                // Find similar workspaces
                var recommendations = await _context.Workspaces
                    .Where(w => w.IsAvailable &&
                                preferredTypes.Contains(w.Type) &&
                                Math.Abs(w.Capacity - avgCapacityNeeded) <= 5)
                    .Include(w => w.Bookings)
                    .OrderByDescending(w => w.Bookings.Count())
                    .Take(count)
                    .ToListAsync();

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspace recommendations for user {UserId}", userId);
                return new List<Workspace>();
            }
        }

        public async Task<List<WorkspaceTypeStatsDto>> GetPopularWorkspaceTypesAsync()
        {
            try
            {
                return await GetWorkspaceTypeStatsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular workspace types");
                return new List<WorkspaceTypeStatsDto>();
            }
        }

        public async Task<List<Booking>> GetUpcomingBookingsAsync(string userId, int count = 5)
        {
            try
            {
                var now = DateTime.UtcNow;
                return await _context.Bookings
                    .Where(b => b.ApplicationUserId == userId && 
                               b.StartTime > now &&
                               b.Status != BookingStatus.Canceled)
                    .Include(b => b.Workspace)
                    .OrderBy(b => b.StartTime)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming bookings for user {UserId}", userId);
                return new List<Booking>();
            }
        }

        public async Task<List<WorkspaceAvailabilityDto>> GetWorkspaceAvailabilityAsync(DateTime date)
        {
            try
            {
                // Get all workspaces and check their availability for the given date
                var workspaces = await _context.Workspaces
                    .Where(w => w.IsAvailable)
                    .Take(10) // Limit to first 10 workspaces for performance
                    .ToListAsync();

                var availabilityList = new List<WorkspaceAvailabilityDto>();

                foreach (var workspace in workspaces)
                {
                    var availability = await GetWorkspaceAvailabilityAsync(workspace.Id, date);
                    if (availability.Any())
                    {
                        availabilityList.AddRange(availability);
                    }
                }

                return availabilityList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspace availability for date {Date}", date);
                return new List<WorkspaceAvailabilityDto>();
            }
        }

        public async Task<PaginatedNotificationsDto> GetUserNotificationsAsync(string userId, int page, int pageSize)
        {
            try
            {
                var totalCount = await _context.Notifications
                    .CountAsync(n => n.ApplicationUserId == userId);

                var notifications = await _context.Notifications
                    .Where(n => n.ApplicationUserId == userId)
                    .OrderByDescending(n => n.DateCreated)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        Message = n.Message,
                        IsRead = n.IsRead,
                        DateCreated = n.DateCreated
                    })
                    .ToListAsync();

                return new PaginatedNotificationsDto
                {
                    Notifications = notifications,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                return new PaginatedNotificationsDto();
            }
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId, string userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.ApplicationUserId == userId);

                if (notification == null)
                {
                    return false;
                }

                notification.IsRead = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
                return false;
            }
        }

        public async Task<bool> ProcessContactFormAsync(ContactFormDto contactDto)
        {
            return await SendContactMessageAsync(contactDto);
        }
    }
}