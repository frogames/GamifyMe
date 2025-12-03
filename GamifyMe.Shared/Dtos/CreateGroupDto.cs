using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Dtos
{
    public class CreateGroupDto
    {
        [Required(ErrorMessage = "Le nom est requis")]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconName { get; set; } = "Group";
        public int? RegistrationDurationHours { get; set; }
    }
}
