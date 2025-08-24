using System.ComponentModel.DataAnnotations;

namespace SpaceFlow.ViewModels
{
    public class ResendConfirmationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
