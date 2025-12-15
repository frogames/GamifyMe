using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Dtos
{
    public class CreateGroupDto
    {
        [Required(ErrorMessage = "Le nom est requis")]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "L'icône est requise.")]
        public string IconName { get; set; } = "fas fa-star";
        
        [Required(ErrorMessage = "La couleur est requise.")]
        public string? Color { get; set; } = "#FFFFFF";
        public bool IsActive { get; set; } = true;
        public int? RegistrationDurationHours { get; set; }
        public string? ImageBase64 { get; set; }
    }
}
