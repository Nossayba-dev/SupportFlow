using SupportFlow.Enums;

namespace SupportFlow.DTOs
{
    public class TicketResponseDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required UserSummaryDto User { get; set; }
        public required CategorySummaryDto Category { get; set; }
        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; }
        public string? Comments { get; set; }
    }
}
