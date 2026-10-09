using SupportFlow.Enums;

namespace SupportFlow.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public UserRole Role { get; set; }

        public required string Email { get; set; }
        public List<TicketSumaryDto> Tickets { get; set; } = new();
    }
}
