using SpaceFlow.Repositories.Models;

namespace SpaceFlow.ViewModels
{
    public class BookingSummaryDto
    {
        public int Id { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public BookingStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public bool IsUpcoming => StartTime > DateTime.Now;
        public bool IsToday => StartTime.Date == DateTime.Today;
    }
}
