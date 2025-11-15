using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Models
{
    // On définit les deux types d'items que vous avez suggérés
    public enum StoreItemType
    {
        Physical, // "À faire valider" par un gestionnaire
        Digital   // "Récompense immédiate" (Boost, Cosmétique, etc.)
    }

    public class StoreItem : IEstablishmentScoped
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EstablishmentId { get; set; } // À quel établissement cet item appartient
        public Establishment Establishment { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string IconName { get; set; } = "fas fa-shopping-bag"; // Icône par défaut

        public int Price { get; set; } // Le coût en DOC-Points

        public int Stock { get; set; } = 999; // Stock (important pour les items physiques)

        public StoreItemType ItemType { get; set; } = StoreItemType.Physical; // Type (Physique/Numérique)

        public bool IsActive { get; set; } = true; // Pour le cacher de la boutique

        // --- Pour les items Numériques ---
        // On peut ajouter un "code d'action" pour dire au jeu quoi faire
        // Ex: "BOOST_XP_24H", "FRAME_HALLOWEEN", "SOUND_SCAN_ROBOT"
        public string? DigitalActionCode { get; set; }

        public List<Order> Orders { get; set; } = new();
        public List<UserInventory> InventoryItems { get; set; } = new();
    }
}