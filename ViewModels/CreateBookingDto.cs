using System;
using System.ComponentModel.DataAnnotations;

namespace SpaceFlow.ViewModels
{
    public class CreateBookingDto
    {
        [Required]
        public int WorkspaceId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }
}