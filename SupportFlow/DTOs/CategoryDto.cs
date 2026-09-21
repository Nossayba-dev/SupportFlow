using System.ComponentModel.DataAnnotations;

namespace SupportFlow.DTOs
{
    public class CategoryDto
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public string Name { get; set; }
    }
}
