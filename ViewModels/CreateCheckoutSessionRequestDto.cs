using System.ComponentModel.DataAnnotations;

namespace SpaceFlow.ViewModels
{
    public class CreateCheckoutSessionRequestDto
    {
        [Required]
        public int BookingId { get; set; }
    }
}