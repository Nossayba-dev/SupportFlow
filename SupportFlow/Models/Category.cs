namespace SupportFlow.Models
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }    
        public List<Ticket> Tickets { get; set; } = new();
    }
}
