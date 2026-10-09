namespace SupportFlow.DTOs
{
    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<TicketSumaryDto> Tickets { get; set; } = new();
    }
}
