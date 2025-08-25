using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceFlow.Repositories.Models;
using SpaceFlow.Services.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SpaceFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create-order/{bookingId}")]
        [Authorize]
        public async Task<IActionResult> CreateOrder(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) 
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var (orderResponse, errorMessage) = await _paymentService.CreateOrderAsync(bookingId, userId);

            if (orderResponse == null)
            {
                return BadRequest(new { message = errorMessage });
            }

            return Ok(orderResponse);
        }

        [HttpPost("verify-payment")]
        [Authorize]
        public async Task<IActionResult> VerifyPayment([FromBody] Dictionary<string, string> paymentAttributes)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var (isSuccess, errorMessage) = await _paymentService.VerifyPaymentAsync(paymentAttributes);

            if (!isSuccess)
            {
                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { 
                status = "Payment verified successfully", 
                message = "Your booking has been confirmed and payment processed.",
                isVerified = true 
            });
        }
        
        [HttpGet("my-history")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Payment>>> GetMyPaymentHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var paymentHistory = await _paymentService.GetPaymentHistoryAsync(userId);
            return Ok(paymentHistory);
        }

        // For testing purposes - allows anonymous access to test the API without authentication
        [HttpPost("create-order-test/{bookingId}")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateOrderTest(int bookingId)
        {
            // For testing, use a dummy user ID
            const string testUserId = "test-user-123";

            var (orderResponse, errorMessage) = await _paymentService.CreateOrderAsync(bookingId, testUserId);

            if (orderResponse == null)
            {
                return BadRequest(new { message = errorMessage });
            }

            return Ok(orderResponse);
        }
    }
}