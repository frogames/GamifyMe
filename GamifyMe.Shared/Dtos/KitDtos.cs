using System;
using System.Collections.Generic;
using GamifyMe.Shared.Models;

namespace GamifyMe.Shared.Dtos
{
    public class ContentKitDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public KitCategory Category { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public Guid TemplateEstablishmentId { get; set; }
        
        public bool HasObjectives { get; set; }
        public bool HasBadges { get; set; }
        public bool HasGroups { get; set; }
        public bool HasStoreItems { get; set; }
        
        public int UsageCount { get; set; }
        public double AverageRating { get; set; }
        
        // Detailed Content
        public List<ObjectiveDto> Objectives { get; set; } = new();
        public List<BadgeDto> Badges { get; set; } = new();
        public List<GroupDto> Groups { get; set; } = new();
        public List<StoreItemDto> StoreItems { get; set; } = new();
    }

    public class CreateKitDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public KitCategory Category { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public Guid TemplateEstablishmentId { get; set; }
    }

    public class UpdateKitDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public KitCategory Category { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class RateKitDto
    {
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; } = string.Empty;
    }

    public class KitFilterDto
    {
        public KitCategory? Category { get; set; }
        public bool? HasObjectives { get; set; }
        public bool? HasBadges { get; set; }
        public bool? HasGroups { get; set; }
        public bool? HasStoreItems { get; set; }
        public string? SearchTerm { get; set; }
    }
}
