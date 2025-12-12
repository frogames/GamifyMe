using System;
using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Models
{
    public class UserBadge : IEstablishmentScoped
    {
        [Key]
        public Guid Id { get; set; }
        public Guid EstablishmentId { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public Guid BadgeId { get; set; }
        public Badge Badge { get; set; } = null!;

        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

        public bool IsFavorite { get; set; } = false;
    }
}
