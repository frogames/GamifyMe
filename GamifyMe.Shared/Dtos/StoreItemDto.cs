using GamifyMe.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System;

namespace GamifyMe.Shared.Dtos
{
    public class StoreItemDto
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string IconName { get; set; } = "fas fa-shopping-bag";

        [Range(0, 1000000)]
        public int Price { get; set; }

        [Range(0, 9999)]
        public int Stock { get; set; } = 999;

        public StoreItemType ItemType { get; set; } = StoreItemType.Physical;
        public bool IsActive { get; set; } = true;
        public string? ImageUrl { get; set; }
        public string? Color { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? DigitalActionCode { get; set; }
        public string? DigitalAssetUrl { get; set; }
        public bool IsUnique { get; set; }
        public bool IsOwned { get; set; } // Computed property for UI
    }
}