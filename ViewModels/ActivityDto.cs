using System;

namespace SpaceFlow.ViewModels
{
    public class ActivityDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // "booking_created", "booking_confirmed", "payment_completed", etc.
        public string Description { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; } // "booking", "payment", "workspace"
    }
}
