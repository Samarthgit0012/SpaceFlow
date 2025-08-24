using System.ComponentModel.DataAnnotations;

namespace SpaceFlow.ViewModels
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
