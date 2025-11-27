using System;

namespace GamifyMe.Shared.Dtos
{
    public class UserInventoryDto
    {
        public Guid Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string IconName { get; set; } = "Help";
        public DateTime AcquiredDate { get; set; }
        
        // Status
        public bool IsActive { get; set; } // Equipped / Running
        public DateTime? ExpiresAt { get; set; }

        public string ItemType { get; set; } = "Physical";
        
        // Digital specifics
        public string? DigitalActionCode { get; set; }
        public string? DigitalAssetUrl { get; set; }
    }
}
