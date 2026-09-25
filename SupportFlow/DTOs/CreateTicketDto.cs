using SupportFlow.Enums;
using System.ComponentModel.DataAnnotations;


namespace SupportFlow.DTOs
{
    public class CreateTicketDto
    {
        [Required]
        [StringLength(50)]
        public string Title { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be a positive integer.")]
        public int CategoryId { get; set; }
        public TicketPriority Priority { get; set; }
        [Required]
        [MaxLength(500, ErrorMessage = "Comments cannot exceed 500 characters.")]
        public string Comments { get; set; }
    }
}
