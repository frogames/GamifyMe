using System;
using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Models
{
    public class KitRating
    {
        public Guid Id { get; set; }

        public Guid KitId { get; set; }
        public ContentKit Kit { get; set; }

        public Guid EstablishmentId { get; set; }
        // We don't necessarily need a navigation property back to Establishment if we don't query it often, 
        // but it's good practice for FK constraints. Using simple Guid for now to avoid circular dependency issues if not needed.
        // Or we can add virtual Establishment Establishment { get; set; } if needed. Let's keep it simple.

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
