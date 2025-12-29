using System.ComponentModel.DataAnnotations;
using GamifyMe.Shared.Models;

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
        public int XpReward { get; set; } = 5;

        [Range(0, 10000, ErrorMessage = "Les points doivent être compris entre 0 et 10 000.")]
        public int DocPointsReward { get; set; } = 5;

        // 1. CORRECTION : Défini à 'true' par défaut, comme tu l'as demandé.
        public bool IsUnique { get; set; } = true;
        public int? FrequencyHours { get; set; }

        public DateTime? EventDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DisplayStartDate { get; set; }
        public DateTime? DisplayEndDate { get; set; }
        public string Location { get; set; } = string.Empty;

        // 2. CORRECTION : Rendu obligatoire, comme tu l'as demandé.
        [Required(ErrorMessage = "L'icône est requise.")]
        public string IconName { get; set; } = "fas fa-star"; // Garde une icône par défaut
        [Required(ErrorMessage = "La couleur est requise.")]
        public string? Color { get; set; } = "#FFFFFF";

        public bool IsActive { get; set; } = true;

        // On n'oublie pas le champ pour les prérequis
        public List<Guid> PrerequisiteObjectiveIds { get; set; } = new List<Guid>();

        public int? LifespanHours { get; set; }



        public ObjectiveCategory Category { get; set; } = ObjectiveCategory.Secondaire;
        public int SortOrder { get; set; }

        public bool IsStreakEnabled { get; set; } = false;
        public int? StreakTerminalHours { get; set; }
        public StreakFrequency StreakFrequency { get; set; } = StreakFrequency.Hourly;
        public string? StreakExcludedDays { get; set; }
        public string? StreakExcludedMonths { get; set; }

        public ValidationMethod AllowedValidationMethods { get; set; }
        public int? RequiredPeerValidations { get; set; }
    }
}