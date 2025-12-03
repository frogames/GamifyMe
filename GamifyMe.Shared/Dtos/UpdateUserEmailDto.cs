using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Dtos
{
    public class UpdateUserEmailDto
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; } = string.Empty;
    }
}
