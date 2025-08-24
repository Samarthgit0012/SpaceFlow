using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceFlow.Services.Interfaces;
using SpaceFlow.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SpaceFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IHomeService homeService, ILogger<HomeController> logger)
        {
            _homeService = homeService;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard data for the home page
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboardData = await _homeService.GetDashboardDataAsync();
            return Ok(dashboardData);
        }

        /// <summary>
        /// Get personalized dashboard for authenticated user
        /// </summary>
        [HttpGet("user-dashboard")]
        [Authorize]
        public async Task<IActionResult> GetUserDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var userDashboard = await _homeService.GetDashboardDataAsync(userId);
            return Ok(userDashboard);
        }

        /// <summary>
        /// Get application statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetApplicationStats()
        {
            var stats = await _homeService.GetApplicationStatsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Get featured workspaces for home page
        /// </summary>
        [HttpGet("featured-workspaces")]
        public async Task<IActionResult> GetFeaturedWorkspaces([FromQuery] int count = 6)
        {
            var featuredWorkspaces = await _homeService.GetFeaturedWorkspacesAsync(count);
            return Ok(featuredWorkspaces);
        }

        /// <summary>
        /// Get recent activities (for logged-in users)
        /// </summary>
        [HttpGet("recent-activities")]
        [Authorize]
        public async Task<IActionResult> GetRecentActivities([FromQuery] int count = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var activities = await _homeService.GetRecentActivitiesAsync(userId, count);
            return Ok(activities);
        }

        /// <summary>
        /// Search workspaces with filters
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchWorkspaces([FromQuery] WorkspaceSearchDto searchDto)
        {
            var searchResults = await _homeService.SearchWorkspacesAsync(searchDto);
            return Ok(searchResults);
        }

        /// <summary>
        /// Get popular workspace types
        /// </summary>
        [HttpGet("popular-types")]
        public async Task<IActionResult> GetPopularWorkspaceTypes()
        {
            var popularTypes = await _homeService.GetPopularWorkspaceTypesAsync();
            return Ok(popularTypes);
        }

        /// <summary>
        /// Get upcoming bookings for user
        /// </summary>
        [HttpGet("upcoming-bookings")]
        [Authorize]
        public async Task<IActionResult> GetUpcomingBookings([FromQuery] int count = 5)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var upcomingBookings = await _homeService.GetUpcomingBookingsAsync(userId, count);
            return Ok(upcomingBookings);
        }

        /// <summary>
        /// Get workspace availability for a specific date
        /// </summary>
        [HttpGet("availability")]
        public async Task<IActionResult> GetWorkspaceAvailability([FromQuery] DateTime date)
        {
            if (date < DateTime.Today)
            {
                return BadRequest(new { message = "Cannot check availability for past dates" });
            }

            var availability = await _homeService.GetWorkspaceAvailabilityAsync(date);
            return Ok(availability);
        }

        /// <summary>
        /// Get notifications for authenticated user
        /// </summary>
        [HttpGet("notifications")]
        [Authorize]
        public async Task<IActionResult> GetUserNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var notifications = await _homeService.GetUserNotificationsAsync(userId, page, pageSize);
            return Ok(notifications);
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPut("notifications/{notificationId}/mark-read")]
        [Authorize]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _homeService.MarkNotificationAsReadAsync(notificationId, userId);
            if (success)
            {
                return Ok(new { message = "Notification marked as read" });
            }

            return NotFound(new { message = "Notification not found" });
        }

        /// <summary>
        /// Get workspace recommendations based on user history
        /// </summary>
        [HttpGet("recommendations")]
        [Authorize]
        public async Task<IActionResult> GetWorkspaceRecommendations([FromQuery] int count = 5)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var recommendations = await _homeService.GetWorkspaceRecommendationsAsync(userId, count);
            return Ok(recommendations);
        }

        /// <summary>
        /// Submit contact form
        /// </summary>
        [HttpPost("contact")]
        public async Task<IActionResult> SubmitContactForm([FromBody] ContactFormDto contactForm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _homeService.ProcessContactFormAsync(contactForm);
            if (success)
            {
                return Ok(new { message = "Contact form submitted successfully. We'll get back to you soon!" });
            }

            return BadRequest(new { message = "Failed to submit contact form. Please try again." });
        }

        /// <summary>
        /// Subscribe to newsletter
        /// </summary>
        [HttpPost("newsletter")]
        public async Task<IActionResult> SubscribeToNewsletter([FromBody] NewsletterSubscriptionDto subscription)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _homeService.SubscribeToNewsletterAsync(subscription);
            if (success)
            {
                return Ok(new { message = "Successfully subscribed to newsletter!" });
            }

            return BadRequest(new { message = "Subscription failed. You may already be subscribed." });
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "SpaceFlow API"
            });
        }
    }
}