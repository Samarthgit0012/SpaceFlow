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
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create-order/{bookingId}")]
        public async Task<IActionResult> CreateOrder(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var (orderResponse, errorMessage) = await _paymentService.CreateOrderAsync(bookingId, userId);

            if (orderResponse == null)
            {
                return BadRequest(new { message = errorMessage });
            }

            return Ok(orderResponse);
        }

        [HttpPost("verify-payment")]
        public async Task<IActionResult> VerifyPayment([FromBody] Dictionary<string, string> paymentAttributes)
        {
            var (isSuccess, errorMessage) = await _paymentService.VerifyPaymentAsync(paymentAttributes);

            if (!isSuccess)
            {
                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { status = "Payment successful (Simulated)" });
        }
        
        [HttpGet("my-history")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetMyPaymentHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var paymentHistory = await _paymentService.GetPaymentHistoryAsync(userId);
            return Ok(paymentHistory);
        }
    }
}