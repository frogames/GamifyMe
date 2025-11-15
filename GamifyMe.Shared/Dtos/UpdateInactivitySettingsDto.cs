using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Dtos
{
    public class UpdateInactivitySettingsDto
    {
        [Range(0, 3650)] // 0 pour désactiver, max 10 ans
        public int ArchiveAfterDays { get; set; }
    }
}