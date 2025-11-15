using System;
using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Models
{
    // Cette table stocke les objets numériques qu'un utilisateur possède
    public class UserInventory : IEstablishmentScoped
    {
        public Guid EstablishmentId { get; set; }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public Guid StoreItemId { get; set; } // L'objet numérique possédé
        public StoreItem StoreItem { get; set; } = null!;

        public DateTime DateAcquired { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = false; // Ex: Le son de scan est-il celui *actif* ?
        public DateTime? ExpiresAt { get; set; } = null; // Ex: Pour le "Boost d'XP 24h"
    }
}