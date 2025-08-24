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
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; }
        public DateTime DateCreated { get; set; }
    }
}