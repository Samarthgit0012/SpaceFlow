using SpaceFlow.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpaceFlow.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetBookingByIdAsync(int id); // Add this
        Task<Booking> CreateBookingAsync(Booking newBooking);
        Task UpdateBookingAsync(Booking bookingToUpdate); // Add this
        Task DeleteBookingAsync(Booking bookingToDelete); // Add this
        Task<bool> HasConflictAsync(int workspaceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null); // Modify this
        Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(string userId);
    }
}