using SpaceFlow.Repositories.Interfaces;
using SpaceFlow.Repositories.Models;
using SpaceFlow.Services.Interfaces;
using SpaceFlow.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpaceFlow.Services.Implementation
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IWorkspaceRepository _workspaceRepo;

        public BookingService(IBookingRepository bookingRepo, IWorkspaceRepository workspaceRepo)
        {
            _bookingRepo = bookingRepo;
            _workspaceRepo = workspaceRepo;
        }

        public async Task<Booking?> GetBookingByIdAsync(int id, string userId)
        {
            var booking = await _bookingRepo.GetBookingByIdAsync(id);
            // Security check: ensure the user owns this booking
            if (booking == null || booking.ApplicationUserId != userId)
            {
                return null;
            }
            return booking;
        }

        public async Task<(Booking? booking, string errorMessage)> UpdateBookingAsync(int id, UpdateBookingDto bookingDto, string userId)
        {
            var existingBooking = await _bookingRepo.GetBookingByIdAsync(id);

            // Security check
            if (existingBooking == null || existingBooking.ApplicationUserId != userId)
            {
                return (null, "Booking not found or you do not have permission to edit it.");
            }

            // Business Rule 1: Check for conflicts, excluding the current booking
            var hasConflict = await _bookingRepo.HasConflictAsync(existingBooking.WorkspaceId, bookingDto.StartTime, bookingDto.EndTime, id);
            if (hasConflict)
            {
                return (null, "The new time slot is unavailable.");
            }

            // Update properties and recalculate price
            existingBooking.StartTime = bookingDto.StartTime;
            existingBooking.EndTime = bookingDto.EndTime;
            // Note: In a real app, you would re-fetch the workspace to ensure the price is current
            var workspace = await _workspaceRepo.GetWorkspaceByIdAsync(existingBooking.WorkspaceId);
            var durationHours = (existingBooking.EndTime - existingBooking.StartTime).TotalHours;
            existingBooking.TotalPrice = (decimal)durationHours * workspace.PricePerHour;

            await _bookingRepo.UpdateBookingAsync(existingBooking);
            return (existingBooking, string.Empty);
        }

        public async Task<(bool success, string errorMessage)> DeleteBookingAsync(int id, string userId)
        {
            var bookingToDelete = await _bookingRepo.GetBookingByIdAsync(id);

            // Security check
            if (bookingToDelete == null || bookingToDelete.ApplicationUserId != userId)
            {
                return (false, "Booking not found or you do not have permission to delete it.");
            }

            await _bookingRepo.DeleteBookingAsync(bookingToDelete);
            return (true, string.Empty);
        }

        // ... (existing CreateBookingAsync and GetMyBookingsAsync methods)
        public async Task<(Booking? booking, string errorMessage)> CreateBookingAsync(CreateBookingDto bookingDto, string userId)
        {
            if (bookingDto.StartTime >= bookingDto.EndTime)
            {
                return (null, "Booking end time must be after the start time.");
            }
            var hasConflict = await _bookingRepo.HasConflictAsync(bookingDto.WorkspaceId, bookingDto.StartTime, bookingDto.EndTime);
            if (hasConflict)
            {
                return (null, "The selected time slot is unavailable.");
            }
            var workspace = await _workspaceRepo.GetWorkspaceByIdAsync(bookingDto.WorkspaceId);
            if (workspace == null)
            {
                return (null, "The selected workspace does not exist.");
            }
            var durationHours = (bookingDto.EndTime - bookingDto.StartTime).TotalHours;
            var totalPrice = (decimal)durationHours * workspace.PricePerHour;
            var newBooking = new Booking
            {
                WorkspaceId = bookingDto.WorkspaceId,
                ApplicationUserId = userId,
                StartTime = bookingDto.StartTime,
                EndTime = bookingDto.EndTime,
                Status = BookingStatus.Pending,
                TotalPrice = totalPrice
            };
            var createdBooking = await _bookingRepo.CreateBookingAsync(newBooking);
            return (createdBooking, string.Empty);
        }

        public async Task<IEnumerable<Booking>> GetMyBookingsAsync(string userId)
        {
            return await _bookingRepo.GetBookingsByUserIdAsync(userId);
        }
    }
}