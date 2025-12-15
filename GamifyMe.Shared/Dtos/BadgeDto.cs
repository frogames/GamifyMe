using GamifyMe.Shared.Models;
using System;

namespace GamifyMe.Shared.Dtos
{
    public class BadgeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconName { get; set; }
        public string? ImageUrl { get; set; }
        public string? Color { get; set; }
        public string Category { get; set; } = "Général";
        public bool IsActive { get; set; }
        public double CurrentValue { get; set; } // Current calculated progress
        
        public BadgeCriteriaType CriteriaType { get; set; }
        public string? CriteriaValue { get; set; }
        public int CriteriaThreshold { get; set; }

        public int XpReward { get; set; }
        public int DocPointsReward { get; set; }
        public Guid? RewardStoreItemId { get; set; }
        
        public bool IsUnlocked { get; set; } // For current user context
        public bool IsFavorite { get; set; } // For current user context
        public DateTime? UnlockedAt { get; set; } // For current user context

        // Expanded details for UI
        public string? RewardStoreItemName { get; set; }
        public StoreItemDto? RewardStoreItem { get; set; }
        public string? TargetObjectiveName { get; set; }
        public List<ObjectiveDto>? RequiredObjectives { get; set; }
        public List<StoreItemDto>? RequiredStoreItems { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
