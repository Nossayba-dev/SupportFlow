using SupportFlow.Enums;

namespace SupportFlow.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public User User { get; set; }
        public Category Category { get; set; }
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public string Comments { get; set; }
        
    }
}
