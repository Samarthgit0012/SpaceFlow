using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceFlow.Repositories.Models
{
    public class Workspace
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // Initialized

        [Required]
        public string Type { get; set; } = string.Empty; // Initialized

        public int Capacity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerHour { get; set; }

        public string? Amenities { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}