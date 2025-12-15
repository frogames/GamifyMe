using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Dtos
{
    public class CreateEstablishmentDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public int MaxUsers { get; set; } = 100;
        public bool IsTemplate { get; set; } = false;
    }
}
