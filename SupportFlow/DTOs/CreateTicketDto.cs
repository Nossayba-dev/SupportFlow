using SupportFlow.Enums;


namespace SupportFlow.DTOs
{
    public class CreateTicketDto
    {        
        public string Title { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public TicketPriority Priority { get; set; }
        public string Comments { get; set; }
    }
}
