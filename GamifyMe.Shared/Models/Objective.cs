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
        public DateTime? EventDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Location { get; set; } = string.Empty; 
        public string IconName { get; set; } = string.Empty;

        // Les objectifs qui doivent être complétés AVANT que celui-ci ne soit accessible
        public ICollection<Objective> Prerequisites { get; set; } = new List<Objective>();

        // Les objectifs qui ont CELUI-CI comme prérequis
        // (EF Core a besoin de cette "navigation inverse" pour créer la relation)
        public ICollection<Objective> IsPrerequisiteFor { get; set; } = new List<Objective>();
    }
}