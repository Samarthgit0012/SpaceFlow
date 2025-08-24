using System.ComponentModel.DataAnnotations;

namespace SpaceFlow.ViewModels
{
    public class UpdateWorkspaceDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        [Range(1, 100)]
        public int Capacity { get; set; }

        [Range(0, 10000)]
        public decimal PricePerHour { get; set; }

        public string? Amenities { get; set; }
        public bool IsAvailable { get; set; }
        public string? ImageUrl { get; set; }
    }
}