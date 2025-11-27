using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Models
{
    public enum BonusType
    {
        Xp,
        Currency
    }

    public class BonusPeriod : IEstablishmentScoped
    {
        public Guid Id { get; set; }
        public Guid EstablishmentId { get; set; }
        public Establishment? Establishment { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public BonusType Type { get; set; }
        public double Multiplier { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
