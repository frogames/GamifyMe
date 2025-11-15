using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Dtos
{
    public class CreateObjectiveDto
    {
        [Required(ErrorMessage = "Le titre est requis.")]
        [StringLength(100, ErrorMessage = "Le titre est trop long (100 caractères max).")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La description est trop longue (500 caractères max).")]
        public string Description { get; set; } = string.Empty;

        [Range(0, 10000, ErrorMessage = "L'XP doit être comprise entre 0 et 10 000.")]
        public int XpReward { get; set; }

        [Range(0, 10000, ErrorMessage = "Les points doivent être compris entre 0 et 10 000.")]
        public int DocPointsReward { get; set; }

        // 1. CORRECTION : Défini à 'true' par défaut, comme tu l'as demandé.
        public bool IsUnique { get; set; } = true;

        public DateTime? EventDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Location { get; set; } = string.Empty;

        // 2. CORRECTION : Rendu obligatoire, comme tu l'as demandé.
        [Required(ErrorMessage = "L'icône est requise.")]
        public string IconName { get; set; } = "fas fa-star"; // Garde une icône par défaut

        public bool IsActive { get; set; } = true;

        // On n'oublie pas le champ pour les prérequis
        public List<Guid> PrerequisiteObjectiveIds { get; set; } = new List<Guid>();
    }
}