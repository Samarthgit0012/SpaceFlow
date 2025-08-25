using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceFlow.Repositories.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = null!;
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;

        [Required]
        public string Message { get; set; } = null!;

        public bool IsRead { get; set; } = false;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}