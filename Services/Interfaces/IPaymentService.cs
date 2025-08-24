using SpaceFlow.Repositories.Models;
using SpaceFlow.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpaceFlow.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<(OrderResponseDto? orderResponse, string? errorMessage)> CreateOrderAsync(int bookingId, string userId);
        Task<(bool isSuccess, string? errorMessage)> VerifyPaymentAsync(Dictionary<string, string> paymentAttributes);
        Task<IEnumerable<Payment>> GetPaymentHistoryAsync(string userId);

    }
}