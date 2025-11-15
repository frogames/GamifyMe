using System.ComponentModel.DataAnnotations.Schema;

namespace GamifyMe.Shared.Models
{
    public class Establishment
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<User> Users { get; set; } = new();
        public Guid? AdminUserId { get; set; } // L'admin de cet établissement (peut être null)
        public Guid? PlanId { get; set; } // Le forfait associé (peut être null)
        public int ArchiveUsersAfterInactiveDays { get; set; } = 365; // Valeur par défaut
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optionnel : Ajouter des relations si nécessaire plus tard
        // public User AdminUser { get; set; }
        // public Plan Plan { get; set; }
    }
}