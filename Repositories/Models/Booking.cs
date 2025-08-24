using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceFlow.Repositories.Models
{
    public enum BookingStatus { Pending, Confirmed, Canceled, Completed }

    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = null!; // Use null-forgiving operator
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; } = null!; // Use null-forgiving operator

        [Required]
        public int WorkspaceId { get; set; }
        [ForeignKey("WorkspaceId")]
        public virtual Workspace Workspace { get; set; } = null!; // Use null-forgiving operator

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public BookingStatus Status { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
    }
}