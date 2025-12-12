namespace GamifyMe.Shared.Dtos
{
    public class UserProfileDetailsDto
    {
        // Infos de base (reprise de ton DTO existant)
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EstablishmentName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string QrCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Progression (Niveau)
        public int Level { get; set; }
        public int CurrentXp { get; set; }
        public int XpForNextLevel { get; set; } // L'objectif à atteindre (ex: 1000)
        public double ProgressPercentage { get; set; } // 0 à 100
        public int Rank { get; set; }

        // Monnaie
        public int CurrencyBalance { get; set; }
        public string CurrencyName { get; set; } = "Crédits"; // "DOC", "Gold", etc.
        public Guid? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? GroupIcon { get; set; }
        public string? GroupColor { get; set; }

        // Historique
        public List<UserActivityLogDto> RecentActivity { get; set; } = new();

        // Custom UI
        public string ActiveUiTheme { get; set; } = Constants.ThemeConstants.Default;
        public string ActiveQrCodeStyle { get; set; } = Constants.ThemeConstants.QrStyleDefault;

        // Boost XP
        public int ActiveBoostMultiplier { get; set; } = 1;
        public DateTime? BoostEndsAt { get; set; }
        public List<BadgeDto> Badges { get; set; } = new();
    }
}