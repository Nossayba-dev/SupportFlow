using SupportFlow.Enums;

namespace SupportFlow.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public User User { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public string? Comments { get; set; }
        
    }
}
