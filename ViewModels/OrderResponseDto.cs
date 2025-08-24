namespace SpaceFlow.ViewModels
{
    public class OrderResponseDto
    {
        public string OrderId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string KeyId { get; set; } = string.Empty; // For the frontend
    }
}