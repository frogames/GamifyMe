using System;
using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Models
{
    public enum OrderStatus
    {
        Pending,    // En attente de récupération (pour les objets physiques)
        Completed,  // Terminé (objet numérique reçu ou objet physique récupéré)
        Cancelled   // Annulé (par un admin)
    }

    public class Order : IEstablishmentScoped
    {
        public Guid EstablishmentId { get; set; }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; } // Le joueur qui a acheté
        public User User { get; set; } = null!;

        [Required]
        public Guid StoreItemId { get; set; } // L'objet qui a été acheté
        public StoreItem StoreItem { get; set; } = null!;

        public int PricePaid { get; set; } // Le prix au moment de l'achat
        public DateTime DatePurchased { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.Completed; // Par défaut "Completed" (pour les items numériques)
    }
}