namespace GamifyMe.Shared.Dtos
{
    public class UserProfileDetailsDto
    {
        // Infos de base (reprise de ton DTO existant)
        public string Username { get; set; } = string.Empty;
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

        // Monnaie
        public int CurrencyBalance { get; set; }
        public string CurrencyName { get; set; } = "Points"; // "DOC", "Gold", etc.

        // Historique
        public List<UserActivityLogDto> RecentActivity { get; set; } = new();
    }
}