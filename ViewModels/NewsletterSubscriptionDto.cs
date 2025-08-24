using System.ComponentModel.DataAnnotations;

namespace SpaceFlow.ViewModels
{
    public class NewsletterSubscriptionDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Name { get; set; }

        public List<string> Interests { get; set; } = new List<string>(); // "workspace_updates", "promotions", "news"
    }
}
