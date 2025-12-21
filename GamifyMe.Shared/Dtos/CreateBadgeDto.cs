using GamifyMe.Shared.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Dtos
{
    public class CreateBadgeDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "Général";

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public string? IconName { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageBase64 { get; set; } // Uploaded image
        public string? Color { get; set; } = "#FFD700";
        public bool IsActive { get; set; } = true;

        public BadgeCriteriaType CriteriaType { get; set; }
        public string? CriteriaValue { get; set; }
        public int CriteriaThreshold { get; set; }

        public int XpReward { get; set; }
        public int DocPointsReward { get; set; }
        public Guid? RewardStoreItemId { get; set; }
        public int SortOrder { get; set; }
        
        public Guid? PrerequisiteBadgeId { get; set; }
        public Guid? PrerequisiteObjectiveId { get; set; }
    }
}
