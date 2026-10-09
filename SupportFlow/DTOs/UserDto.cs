using System.ComponentModel.DataAnnotations;

namespace SupportFlow.DTOs
{
    public class UserDto
    {
        [Required]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public required string Name { get; set; }       
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string Email { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public required string Password { get; set; }
    }
}
