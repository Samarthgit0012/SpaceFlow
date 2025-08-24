using Microsoft.EntityFrameworkCore;
using SpaceFlow.Data;
using SpaceFlow.Repositories.Interfaces;
using SpaceFlow.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceFlow.Repositories.Implementation
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings.FindAsync(id);
        }

        public async Task<Booking> CreateBookingAsync(Booking newBooking)
        {
            await _context.Bookings.AddAsync(newBooking);
            await _context.SaveChangesAsync();
            return newBooking;
        }

        public async Task UpdateBookingAsync(Booking bookingToUpdate)
        {
            _context.Bookings.Update(bookingToUpdate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBookingAsync(Booking bookingToDelete)
        {
            _context.Bookings.Remove(bookingToDelete);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasConflictAsync(int workspaceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null)
        {
            var query = _context.Bookings
                .Where(b =>
                    b.WorkspaceId == workspaceId &&
                    b.Status != BookingStatus.Canceled &&
                    startTime < b.EndTime &&
                    endTime > b.StartTime);

            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.Id != excludeBookingId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(string userId)
        {
            return await _context.Bookings
                .Where(b => b.ApplicationUserId == userId)
                .Include(b => b.Workspace)
                .ToListAsync();
        }
    }
}