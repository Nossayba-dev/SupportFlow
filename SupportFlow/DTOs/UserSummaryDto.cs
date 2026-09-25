using SupportFlow.Enums;

namespace SupportFlow.DTOs
{
    public class UserSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public UserRole Role { get; set; }
        public string Email { get; set; }
    }

}
