using SpaceFlow.Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpaceFlow.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> CreatePaymentAsync(Payment newPayment);
        Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(string userId);
    }
}