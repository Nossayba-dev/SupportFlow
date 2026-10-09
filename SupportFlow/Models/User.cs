using SupportFlow.Enums;
namespace SupportFlow.Models
{
    public class User
    {
        public int Id{ get; set; }
        public required string Name { get; set; }
        public UserRole Role { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public List<Ticket> Tickets { get; set; } = new();
    }
}
