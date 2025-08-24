using System.ComponentModel.DataAnnotations;

namespace SpaceFlow.ViewModels
{
    public class DeleteAccountDto
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? Reason { get; set; }
    }
}
