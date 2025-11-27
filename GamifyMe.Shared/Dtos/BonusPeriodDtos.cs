using GamifyMe.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Dtos
{
    public class BonusPeriodDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BonusType Type { get; set; }
        public double Multiplier { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateBonusPeriodDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BonusType Type { get; set; }
        public double Multiplier { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateBonusPeriodDto : CreateBonusPeriodDto { }
}
