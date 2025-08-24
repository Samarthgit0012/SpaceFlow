using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

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
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Initialized

        [Range(1, 1000)]
        public int Capacity { get; set; }

        [Range(0.01, 10000.00)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerHour { get; set; }

        [StringLength(500)]
        public string? Amenities { get; set; }

        public bool IsAvailable { get; set; } = true;

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}