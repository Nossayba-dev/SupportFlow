using SupportFlow.Enums;
using System.ComponentModel.DataAnnotations;
namespace SupportFlow.DTOs
{
    public class UpdateUserRoleDto
    {
        [Required]
        public UserRole Role { get; set; }
    }
}
