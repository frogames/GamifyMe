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
    }
}