using System;
using System.ComponentModel.DataAnnotations;

namespace SpaceFlow.ViewModels
{
    public class UpdateBookingDto
    {
        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }
}