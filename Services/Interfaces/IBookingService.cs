using SpaceFlow.Repositories.Models;
using SpaceFlow.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpaceFlow.Services.Interfaces
{
    public interface IBookingService
    {
        Task<(Booking? booking, string errorMessage)> CreateBookingAsync(CreateBookingDto bookingDto, string userId);
        Task<IEnumerable<Booking>> GetMyBookingsAsync(string userId);
        Task<Booking?> GetBookingByIdAsync(int id, string userId); // Add this
        Task<(Booking? booking, string errorMessage)> UpdateBookingAsync(int id, UpdateBookingDto bookingDto, string userId); // Add this
        Task<(bool success, string errorMessage)> DeleteBookingAsync(int id, string userId); // Add this
    }
}