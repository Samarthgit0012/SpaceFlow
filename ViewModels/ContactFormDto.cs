using System.ComponentModel.DataAnnotations;

namespace SpaceFlow.ViewModels
{
    public class ContactFormDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Company { get; set; }
    }
}
