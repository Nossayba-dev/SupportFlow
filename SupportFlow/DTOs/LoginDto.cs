using System.ComponentModel.DataAnnotations;

namespace SupportFlow.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}
