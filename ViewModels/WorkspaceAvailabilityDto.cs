using System;
using System.Collections.Generic;

namespace SpaceFlow.ViewModels
{
    public class WorkspaceAvailabilityDto
    {
        public int WorkspaceId { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public List<TimeSlotDto> AvailableSlots { get; set; } = new List<TimeSlotDto>();
        public List<TimeSlotDto> BookedSlots { get; set; } = new List<TimeSlotDto>();
        public List<TimeSlotDto> TimeSlots { get; set; } = new List<TimeSlotDto>();
    }
}
