using SupportFlow.Enums;
using System.ComponentModel.DataAnnotations;

namespace SupportFlow.DTOs
{
    public class UpdateTicketDto
    {
        [Required]
        [StringLength(50)]
        public string Title { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be a positive integer.")]
        public int CategoryId { get; set; }
        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; }
        [Required]
        [MaxLength(500, ErrorMessage = "Comments cannot exceed 500 characters.")]
        public string Comments { get; set; }
    }
}
