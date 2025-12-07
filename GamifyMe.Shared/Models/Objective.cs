namespace GamifyMe.Shared.Models
{
    public class Objective : IEstablishmentScoped
    {
        public Guid EstablishmentId { get; set; }

        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int XpReward { get; set; }
        public int DocPointsReward { get; set; } // Pour l'instant, on gère une seule monnaie de récompense pour rester simple
        public Guid CreatedById { get; set; } // L'ID du documentaliste qui a créé l'objectif
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Validation> Validations { get; set; } = new();
        public bool IsUnique { get; set; } = true;
        public int? FrequencyHours { get; set; } = 24;
        public DateTime? EventDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DisplayStartDate { get; set; }
        public DateTime? DisplayEndDate { get; set; }
        public string Location { get; set; } = string.Empty; 
        public string IconName { get; set; } = string.Empty;
        public string? Color { get; set; } // Hex Color Code
        public int? LifespanHours { get; set; }

        // Les objectifs qui doivent être complétés AVANT que celui-ci ne soit accessible
        public ICollection<Objective> Prerequisites { get; set; } = new List<Objective>();

        // Les objectifs qui ont CELUI-CI comme prérequis
        // (EF Core a besoin de cette "navigation inverse" pour créer la relation)
        public ICollection<Objective> IsPrerequisiteFor { get; set; } = new List<Objective>();



        // --- Relations ---
        public virtual ICollection<UserObjective> UserObjectives { get; set; } = new List<UserObjective>();

        public ObjectiveCategory Category { get; set; } = ObjectiveCategory.Secondaire;
        public int SortOrder { get; set; } = 0;
    }

    public enum ObjectiveCategory
    {
        Principal,
        Evenement,
        Secondaire,
        Onboarding,
        Rechargement
    }
}