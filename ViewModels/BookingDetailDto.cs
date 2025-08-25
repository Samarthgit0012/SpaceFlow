using SpaceFlow.Repositories.Models;

namespace SpaceFlow.ViewModels
{
    public class BookingDetailDto
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public int WorkspaceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public BookingStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
        
        // Workspace details (without circular reference)
        public WorkspaceBasicDto? Workspace { get; set; }
        
        // Computed properties
        public TimeSpan Duration => EndTime - StartTime;
        public bool IsUpcoming => StartTime > DateTime.Now;
        public bool IsToday => StartTime.Date == DateTime.Today;
    }

    public class WorkspaceBasicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal PricePerHour { get; set; }
        public string? Amenities { get; set; }
        public bool IsAvailable { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}