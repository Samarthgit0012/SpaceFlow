namespace SpaceFlow.ViewModels
{
    public class TimeSlotDto
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public int? BookingId { get; set; }
    }
}
