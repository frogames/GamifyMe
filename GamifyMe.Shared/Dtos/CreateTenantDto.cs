using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Dtos
{
    public class CreateTenantDto
    {
        [Required]
        public string EstablishmentName { get; set; } = string.Empty;

        [Required]
        public string AdminFirstName { get; set; } = string.Empty;

        [Required]
        public string AdminEmail { get; set; } = string.Empty;

        [Required]
        public string AdminPassword { get; set; } = string.Empty;
        
        public string PlanId { get; set; } = "free"; 
    }
}
