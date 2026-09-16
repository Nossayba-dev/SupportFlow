using SupportFlow.Enums;

namespace SupportFlow.DTOs
{
    public class TicketResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public UserSummaryDto User { get; set; }
        public CategorySummaryDto Category { get; set; }
        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; }
        public string Comments { get; set; }
    }
}
