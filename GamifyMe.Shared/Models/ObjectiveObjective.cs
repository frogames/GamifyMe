using System.ComponentModel.DataAnnotations.Schema;

namespace GamifyMe.Shared.Models
{
    // C'est l'entité de la table de jointure Many-to-Many
    // pour les prérequis d'objectifs
    public class ObjectiveObjective
    {
        // Clé étrangère vers l'objectif principal (ex: "Débloquer le Tapis")
        public Guid IsPrerequisiteForId { get; set; }

        [ForeignKey("IsPrerequisiteForId")]
        public Objective IsPrerequisiteForObjective { get; set; } = null!;

        // Clé étrangère vers l'objectif qui est un prérequis (ex: "Faire 10 squats")
        public Guid PrerequisitesId { get; set; }

        [ForeignKey("PrerequisitesId")]
        public Objective PrerequisiteObjective { get; set; } = null!;
    }
}