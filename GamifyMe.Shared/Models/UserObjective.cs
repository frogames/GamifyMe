using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamifyMe.Shared.Models
{
    public class UserObjective : IEstablishmentScoped
    {
        [Key]
        public Guid Id { get; set; }
        public Guid EstablishmentId { get; set; }

        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public Guid ObjectiveId { get; set; }
        [ForeignKey("ObjectiveId")]
        public Objective Objective { get; set; } = null!;

        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public int Progress { get; set; }
    }
}
