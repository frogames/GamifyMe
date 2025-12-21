using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Models
{
    public enum KitCategory
    {
        Sport,
        Mediatheque,
        Evenement,
        Famille,
        Autre
    }

    public class ContentKit
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public KitCategory Category { get; set; } = KitCategory.Autre;

        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public Guid TemplateEstablishmentId { get; set; }
        public Establishment TemplateEstablishment { get; set; }

        // Content Flags (Auto-detected)
        public bool HasObjectives { get; set; }
        public bool HasBadges { get; set; }
        public bool HasGroups { get; set; }
        public bool HasStoreItems { get; set; }

        // Stats
        public int UsageCount { get; set; }
        public double AverageRating { get; set; }

        public List<KitRating> Ratings { get; set; } = new List<KitRating>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
