using System;

namespace GamifyMe.Shared.Dtos
{
    // Ce DTO représente un objectif tel qu'affiché au joueur
    public class ObjectiveDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        // Iconographie (comme tu l'as mentionné dans ta V1)
        public string? IconName { get; set; }
        public string? Color { get; set; }

        // Récompenses
        public int XpReward { get; set; }
        public int DocPointsReward { get; set; } // On va devoir généraliser ça

        // Métadonnées
        public string? Location { get; set; }
        public DateTime? EventDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DisplayStartDate { get; set; }
        public DateTime? DisplayEndDate { get; set; }

        // Statut pour le joueur
        public bool IsUnique { get; set; }
        public int? FrequencyHours { get; set; }
        public bool IsAlreadyCompleted { get; set; } // Très important pour l'UI

        public bool IsActive { get; set; }

        public List<Guid> PrerequisiteObjectiveIds { get; set; } = new List<Guid>();
        
        public int? LifespanHours { get; set; }
        public DateTime? NextAvailableDate { get; set; }
        public int? ChainPosition { get; set; }
        public int? ChainLength { get; set; }
        public DateTime? ExpirationDate { get; set; }

        // Onboarding
        public bool IsOnboarding { get; set; }
        public Guid? NextOnboardingObjectiveId { get; set; }

        public double ActiveMultiplier { get; set; } = 1.0;
        public string? BonusLabel { get; set; }
    }
}