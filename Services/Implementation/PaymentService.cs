using SpaceFlow.Repositories.Interfaces;
using SpaceFlow.Repositories.Models;
using SpaceFlow.Services.Interfaces;
using SpaceFlow.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpaceFlow.Services.Implementation
{
    public class DummyPaymentService : IPaymentService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IPaymentRepository _paymentRepo;

        public DummyPaymentService(IBookingRepository bookingRepo, IPaymentRepository paymentRepo)
        {
            _bookingRepo = bookingRepo;
            _paymentRepo = paymentRepo;
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

            // FAKE A SUCCESSFUL ORDER CREATION
            var fakeOrder = new OrderResponseDto
            {
                OrderId = "order_DUMMY_" + Guid.NewGuid().ToString(),
                Amount = (long)(booking.TotalPrice * 100),
                Currency = "INR",
                KeyId = "rzp_test_DUMMYKEY"
            };

            return (fakeOrder, null);
        }

        public async Task<(bool isSuccess, string? errorMessage)> VerifyPaymentAsync(Dictionary<string, string> paymentAttributes)
        {
            // In this dummy service, we assume the payment is always successful.
            // We need a booking to update, so we'll just find the most recent one for the user.
            // Note: This is simplified for testing.

            var bookingIdStr = paymentAttributes.GetValueOrDefault("bookingId");
            if (int.TryParse(bookingIdStr, out int bookingId))
            {
                var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);
                if (booking != null)
                {
                    booking.Status = BookingStatus.Confirmed;
                    await _bookingRepo.UpdateBookingAsync(booking);

                    var newPayment = new Payment
                    {
                        ApplicationUserId = booking.ApplicationUserId,
                        BookingId = booking.Id,
                        Amount = booking.TotalPrice,
                        PaymentDate = DateTime.UtcNow,
                        StripePaymentIntentId = "pay_DUMMY_" + Guid.NewGuid().ToString() // Reusing field for dummy ID
                    };
                    await _paymentRepo.CreatePaymentAsync(newPayment);
                }
            }

            return (true, null);
        }
    }
}