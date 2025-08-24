namespace SpaceFlow.ViewModels
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime DateCreated { get; set; }
        public string Type { get; set; } = string.Empty; // "info", "warning", "success", "error"
        public string? ActionUrl { get; set; }
    }
}
