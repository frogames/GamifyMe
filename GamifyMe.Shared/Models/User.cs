using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamifyMe.Shared.Models
{
    public class User : IEstablishmentScoped
    {
        public Guid Id { get; set; }

        public Guid EstablishmentId { get; set; }

        public Establishment Establishment { get; set; } = null!;

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string Status { get; set; } = "active";

        public string QrCode { get; set; } = string.Empty;

        public DateTime LastActivityAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<Wallet> Wallets { get; set; } = new();

        [InverseProperty("User")]
        public List<Validation> ValidationsReceived { get; set; } = new();

        public List<Order> Orders { get; set; } = new();
        public List<UserInventory> Inventory { get; set; } = new();

        public Guid? GroupId { get; set; }
        public Group? Group { get; set; }

        public List<UserObjective> UserObjectives { get; set; } = new();
        public List<UserBadge> UserBadges { get; set; } = new();

        // --- PROPRIÉTÉS CALCULÉES / NON MAPPÉES ---
        [NotMapped]
        public int Level { get; set; }

        public int CurrentXp { get; set; }

        [NotMapped]
        public int CurrencyBalance { get; set; }

        public bool HasCompletedOnboarding { get; set; } = false;
        public bool IsEmailConfirmed { get; set; } = false; // Par défaut, non confirmé
        public string? EmailConfirmationToken { get; set; }
    }
}