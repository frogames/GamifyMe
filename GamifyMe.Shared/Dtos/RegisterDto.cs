using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Dtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Le nom utilisateur est requis.")] 
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'adresse email est requise.")] 
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis.")] 
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'établissement est requis'.")] 
        public Guid EstablishmentId { get; set; }
    }
}