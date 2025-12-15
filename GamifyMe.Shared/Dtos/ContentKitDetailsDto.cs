using System;
using System.Collections.Generic;

namespace GamifyMe.Shared.Dtos
{
    public class ContentKitDetailsDto
    {
        public Guid EstablishmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public int ObjectivesCount { get; set; }
        public List<ObjectiveDto> Objectives { get; set; } = new();

        public int BadgesCount { get; set; }
        public List<BadgeDto> Badges { get; set; } = new();

        public int GroupsCount { get; set; }
        public List<GroupDto> Groups { get; set; } = new();

        public int StoreItemsCount { get; set; }
        public List<StoreItemDto> StoreItems { get; set; } = new();
    }
}
