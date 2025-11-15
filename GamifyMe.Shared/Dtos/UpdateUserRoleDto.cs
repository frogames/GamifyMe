using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Dtos
{
    public class UpdateUserRoleDto
    {
        [Required]
        public string NewRole { get; set; } = string.Empty;
    }
}