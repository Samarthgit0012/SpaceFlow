using Microsoft.EntityFrameworkCore;
using SpaceFlow.Data;
using SpaceFlow.Repositories.Interfaces;
using SpaceFlow.Repositories.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceFlow.Repositories.Implementation
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> CreatePaymentAsync(Payment newPayment)
        {
            await _context.Payments.AddAsync(newPayment);
            await _context.SaveChangesAsync();
            return newPayment;
        }
        
        public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(string userId)
        {
            return await _context.Payments
                .Where(p => p.ApplicationUserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .Include(p => p.Booking) // Optionally include related booking info
                .ThenInclude(b => b.Workspace) // Optionally include workspace info from the booking
                .ToListAsync();
        }
    }
}