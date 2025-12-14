using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Dtos
{
    public class UpdateUserAdminDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User";

        public int XpBalance { get; set; }
        public int CurrencyBalance { get; set; }
    }
}
