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
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // GET: api/Booking/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetBooking(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _bookingService.GetBookingByIdAsync(id, userId);

            if (booking == null)
            {
                return NotFound();
            }

            return Ok(booking);
        }

        // PUT: api/Booking/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] UpdateBookingDto bookingDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (updatedBooking, errorMessage) = await _bookingService.UpdateBookingAsync(id, bookingDto, userId);

            if (updatedBooking == null)
            {
                // Use 404 for not found, 409 for conflict
                if (errorMessage.Contains("unavailable"))
                {
                    return Conflict(new { message = errorMessage });
                }
                return NotFound(new { message = errorMessage });
            }

            return Ok(updatedBooking);
        }

        // DELETE: api/Booking/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, errorMessage) = await _bookingService.DeleteBookingAsync(id, userId);

            if (!success)
            {
                return NotFound(new { message = errorMessage });
            }

            return NoContent(); // Standard response for a successful delete
        }

        // ... (existing POST and GET my-bookings endpoints)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto bookingDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            var (createdBooking, errorMessage) = await _bookingService.CreateBookingAsync(bookingDto, userId);
            if (createdBooking == null)
            {
                if (errorMessage.Contains("unavailable"))
                {
                    return Conflict(new { message = errorMessage });
                }
                return BadRequest(new { message = errorMessage });
            }
            return Ok(createdBooking);
        }

        [HttpGet("my-bookings")]
        [Authorize]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            var bookings = await _bookingService.GetMyBookingsAsync(userId);
            return Ok(bookings);
        }

        // Test endpoints for anonymous access
        [HttpGet("test/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBookingTest(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id, "test-user-123");

            if (booking == null)
            {
                return NotFound(new { message = "Booking not found" });
            }

            return Ok(booking);
        }

        [HttpGet("test-all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllBookingsTest()
        {
            var bookings = await _bookingService.GetMyBookingsAsync("test-user-123");
            return Ok(bookings);
        }
    }
}