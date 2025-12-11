using System.ComponentModel.DataAnnotations.Schema;

namespace GamifyMe.Shared.Models
{
    public class Group : IEstablishmentScoped
    {
        public Guid Id { get; set; }
        public Guid EstablishmentId { get; set; }
        public Establishment Establishment { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconName { get; set; } = "Groups";
        public string? Color { get; set; }
        public int TotalXp { get; set; } = 0;
        public int? RegistrationDurationHours { get; set; } = 0; // 0 or null = unlimited
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [InverseProperty("Group")]
        public List<User> Members { get; set; } = new();
    }
}
