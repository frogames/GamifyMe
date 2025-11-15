using System.ComponentModel.DataAnnotations.Schema;

namespace GamifyMe.Shared.Models
{
    public class Validation : IEstablishmentScoped
    {
        public Guid EstablishmentId { get; set; }

        public Guid Id { get; set; }
        public Guid ObjectiveId { get; set; } // L'objectif qui a été validé
        public Guid UserId { get; set; } // L'utilisateur qui a été récompensé
        public Guid ValidatedById { get; set; } // Le documentaliste qui a fait le scan
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public Objective Objective { get; set; } = null!;
        [ForeignKey("UserId")] // Indique que UserId est la clé pour ScannedUser
        public User ScannedUser { get; set; } = null!;
    }
}