namespace GamifyMe.Shared.Dtos
{
    public class ValidationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        // --- NOUVEAUX CHAMPS NÉCESSAIRES ---
        public int RewardXp { get; set; }
        public int RewardCurrency { get; set; }
        public int UserNewLevel { get; set; }
        public int UserNewBalance { get; set; }

        // --- DIGITAL REWARDS ---
        public string? ScanSoundUrl { get; set; }
        
        public List<BadgeDto> NewAvailableBadges { get; set; } = new();
        public List<ObjectiveDto> NewAvailableObjectives { get; set; } = new();

        // --- OVERRIDE LOGIC ---
        public bool RequiresConfirmation { get; set; }
    }
}