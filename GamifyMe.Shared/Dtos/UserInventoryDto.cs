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
        public bool IsUsed { get; set; }
        public DateTime? UsedDate { get; set; }
        public string ItemType { get; set; } = "Physical";
    }
}
