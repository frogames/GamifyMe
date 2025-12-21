using System;
using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Models
{
    public class Badge : IEstablishmentScoped
    {
        [Key]
        public Guid Id { get; set; }

        public Guid EstablishmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        // Visuals
        public string? IconName { get; set; } // MudBlazor Icon
        public string? ImageUrl { get; set; } // Uploaded image
        public string? Color { get; set; } = "#FFD700"; // Gold by default
        public bool IsActive { get; set; } = true;
        
        [StringLength(50)]
        public string Category { get; set; } = "Général";

        // Logic
        public BadgeCriteriaType CriteriaType { get; set; }
        
        // Stores the target value (e.g., "10" for level 10, "Guid-Of-Objective" for specific objective)
        public string? CriteriaValue { get; set; }
        
        // For "ObjectiveSpecificCount", we usually need the ObjectiveId AND the Count.
        // We can store ObjectiveId in CriteriaValue, and the count in a secondary field or parse distinct format.
        // Let's use a secondary field for clarity if needed, or just CriteriaThreshold.
        public int CriteriaThreshold { get; set; } // e.g. Level 10, 5000 XP, 10 Validations

        // Rewards
        public int XpReward { get; set; }
        public int DocPointsReward { get; set; }
        public Guid? RewardStoreItemId { get; set; } // Optional item reward

        // Prerequisites
        public Guid? PrerequisiteBadgeId { get; set; }
        public Guid? PrerequisiteObjectiveId { get; set; }

        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
