using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Dtos
{
    public class UpdateUserNameDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;
    }
}
