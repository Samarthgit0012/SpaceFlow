using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceFlow.Repositories.Models
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = null!;
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;

        [Required]
        public string PlanName { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; } = true;
    }
}