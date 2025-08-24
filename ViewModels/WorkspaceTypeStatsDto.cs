namespace SpaceFlow.ViewModels
{
    public class WorkspaceTypeStatsDto
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
        public int BookingsCount { get; set; }
        public decimal Revenue { get; set; }
        public double Percentage { get; set; }
    }
}
