using System.ComponentModel.DataAnnotations.Schema;

namespace GamifyMe.Shared.Models
{
    public class Validation : IEstablishmentScoped
    {
        public Guid EstablishmentId { get; set; }

        public Guid Id { get; set; }
        public Guid ObjectiveId { get; set; }

        public Guid UserId { get; set; }

        public Guid ValidatedById { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public Objective Objective { get; set; } = null!;

        // --- CORRECTION ICI : On renomme 'ScannedUser' en 'User' ---
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        // ----------------------------------------------------------
    }
}