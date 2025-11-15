using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Dtos
{
    public class UpdateUserProfileDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)] // Ajouter des contraintes
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}