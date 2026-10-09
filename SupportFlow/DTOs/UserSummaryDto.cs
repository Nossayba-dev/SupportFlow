using SupportFlow.Enums;

namespace SupportFlow.DTOs
{
    public class UserSummaryDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public UserRole Role { get; set; }
        public required string Email { get; set; }
    }

}
