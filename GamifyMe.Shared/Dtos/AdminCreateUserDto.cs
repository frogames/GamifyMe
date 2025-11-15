using System.ComponentModel.DataAnnotations; // Nécessaire pour [Required]

namespace GamifyMe.Shared.Dtos
{
    public class AdminCreateUserDto
    {
        [Required] // On ajoute des validations simples
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)] // On peut ajouter une longueur minimale pour le mot de passe
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // Le rôle à assigner
    }
}