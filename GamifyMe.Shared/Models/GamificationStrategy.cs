using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamifyMe.Shared.Models
{
    public class GamificationStrategy
    {
        public Guid Id { get; set; }

        public Guid EstablishmentId { get; set; }
        [ForeignKey("EstablishmentId")]
        public Establishment? Establishment { get; set; }

        public EngagementEngine EngagementEngine { get; set; } = EngagementEngine.Mastery;
        public FeedbackFrequency FeedbackFrequency { get; set; } = FeedbackFrequency.Immediate;
        
        public CycleType CycleType { get; set; } = CycleType.Infinite;
        public int CycleDurationMonths { get; set; } = 12;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
