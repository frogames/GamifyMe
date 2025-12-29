using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamifyMe.Shared.Models
{
    public class PeerObjectiveSignature : IEstablishmentScoped
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid EstablishmentId { get; set; }

        public Guid ObjectiveId { get; set; }
        [ForeignKey("ObjectiveId")]
        public Objective Objective { get; set; } = null!;

        public Guid PerformerUserId { get; set; }
        [ForeignKey("PerformerUserId")]
        public User PerformerUser { get; set; } = null!;

        public Guid WitnessUserId { get; set; }
        [ForeignKey("WitnessUserId")]
        public User WitnessUser { get; set; } = null!;

        public DateTime SignedAt { get; set; } = DateTime.UtcNow;
    }
}
