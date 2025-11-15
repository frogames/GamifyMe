using GamifyMe.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System; // <-- Ajoute ce 'using' pour Guid

namespace GamifyMe.Shared.Dtos
{
    public class StoreItemDto
    {
        // 👇 AJOUTE CETTE LIGNE 👇
        // L'ID est nécessaire pour afficher/modifier les objets existants
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        // ... (le reste de ton fichier est parfait et reste inchangé) ...
        public string IconName { get; set; } = "fas fa-shopping-bag";

        [Range(0, 1000000)]
        public int Price { get; set; }

        [Range(0, 9999)]
        public int Stock { get; set; } = 999;

        public StoreItemType ItemType { get; set; } = StoreItemType.Physical;
        public bool IsActive { get; set; } = true;
        public string? DigitalActionCode { get; set; }
    }
}