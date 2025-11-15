using System.ComponentModel.DataAnnotations; // <-- AJOUTE ÇA

namespace GamifyMe.Shared.Dtos
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "L'email est requis.")] // <-- AJOUTE ÇA
        [EmailAddress(ErrorMessage = "Format d'email invalide.")] // <-- AJOUTE ÇA
        public string? Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis.")] // <-- AJOUTE ÇA
        public string? Password { get; set; }
    }
}