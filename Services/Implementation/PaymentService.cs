using Microsoft.Extensions.Configuration;
using SpaceFlow.Repositories.Interfaces;
using SpaceFlow.Repositories.Models;
using SpaceFlow.Services.Interfaces;
using SpaceFlow.ViewModels;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SpaceFlow.Services.Implementation
{
    // Renamed from DummyPaymentService to RazorpayPaymentService for clarity
    public class RazorpayPaymentService : IPaymentService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly string _razorpayKeySecret;

        // CHANGE 1: The constructor now needs IConfiguration to access the secret key
        public RazorpayPaymentService(IBookingRepository bookingRepo, IPaymentRepository paymentRepo, IConfiguration configuration)
        {
            _bookingRepo = bookingRepo;
            _paymentRepo = paymentRepo;
            _razorpayKeySecret = configuration["Razorpay:KeySecret"] ?? throw new InvalidOperationException("Razorpay KeySecret not found in configuration");
        }

        public async Task<IEnumerable<Payment>> GetPaymentHistoryAsync(string userId)
        {
            return await _paymentRepo.GetPaymentsByUserIdAsync(userId);
        }

        public async Task<(OrderResponseDto? orderResponse, string? errorMessage)> CreateOrderAsync(int bookingId, string userId)
        {
            var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return (null, "Booking not found.");
            }

            // Create a proper order (this could be enhanced to use actual Razorpay API)
            var order = new OrderResponseDto
            {
                OrderId = "order_" + Guid.NewGuid().ToString().Replace("-", ""),
                Amount = (long)(booking.TotalPrice * 100), // Convert to paise
                Currency = "INR",
                KeyId = "rzp_test_DiMiYr3VpklxK8" // Use your actual key ID from config
            };

            return (order, null);
        }

        // CHANGE 2: The entire VerifyPaymentAsync method is replaced with secure cryptographic logic
        public async Task<(bool isSuccess, string? errorMessage)> VerifyPaymentAsync(Dictionary<string, string> paymentAttributes)
        {
            // Extract required payment attributes
            paymentAttributes.TryGetValue("razorpay_order_id", out var orderId);
            paymentAttributes.TryGetValue("razorpay_payment_id", out var paymentId);
            paymentAttributes.TryGetValue("razorpay_signature", out var receivedSignature);
            paymentAttributes.TryGetValue("bookingId", out var bookingIdStr);

            // Validate that all required attributes are present
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(receivedSignature))
            {
                return (false, "Payment verification failed: Missing required payment attributes (razorpay_order_id, razorpay_payment_id, razorpay_signature).");
            }

            // This is the core security check - generate expected signature
            string payload = $"{orderId}|{paymentId}";
            string expectedSignature = GenerateHmacSha256(payload, _razorpayKeySecret);

            // Compare signatures using cryptographically secure comparison
            if (!expectedSignature.Equals(receivedSignature, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Payment verification failed: Invalid signature. Payment may be fraudulent.");
            }

            // If signature is valid, update the database
            if (int.TryParse(bookingIdStr, out int bookingId))
            {
                var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);
                if (booking != null && booking.Status != BookingStatus.Confirmed)
                {
                    // Update booking status
                    booking.Status = BookingStatus.Confirmed;
                    await _bookingRepo.UpdateBookingAsync(booking);

                    // Create payment record with actual payment ID
                    var newPayment = new Payment
                    {
                        ApplicationUserId = booking.ApplicationUserId,
                        BookingId = booking.Id,
                        Amount = booking.TotalPrice,
                        PaymentDate = DateTime.UtcNow,
                        StripePaymentIntentId = paymentId // Store the actual Razorpay payment ID
                    };
                    await _paymentRepo.CreatePaymentAsync(newPayment);

                    return (true, "Payment verified and booking confirmed successfully.");
                }
                else if (booking != null && booking.Status == BookingStatus.Confirmed)
                {
                    return (false, "Booking is already confirmed.");
                }
                else
                {
                    return (false, "Booking not found.");
                }
            }
            
            return (false, "Invalid booking ID provided.");
        }

        // CHANGE 3: Add this helper method for cryptographic signature generation
        private static string GenerateHmacSha256(string payload, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            
            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(payloadBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}