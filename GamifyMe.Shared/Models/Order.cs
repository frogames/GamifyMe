using System;
using System.ComponentModel.DataAnnotations;

namespace GamifyMe.Shared.Models
{
    public enum OrderStatus
    {
        Pending,    // En attente
        Completed,  // Terminé / Livré
        Cancelled   // Annulé
    }

    public class Order : IEstablishmentScoped
    {
        public Guid EstablishmentId { get; set; }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public Guid StoreItemId { get; set; }
        public StoreItem StoreItem { get; set; } = null!;

        public int PricePaid { get; set; }
        public DateTime DatePurchased { get; set; } = DateTime.UtcNow;

        // --- CORRECTION ICI : Ajout de la date de fin ---
        public DateTime? DateCompleted { get; set; }
        // ------------------------------------------------

        public OrderStatus Status { get; set; } = OrderStatus.Completed;
    }
}