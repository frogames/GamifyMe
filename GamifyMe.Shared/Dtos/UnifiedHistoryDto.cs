namespace GamifyMe.Shared.Dtos
{
    public class UnifiedHistoryDto
    {
        public DateTime Date { get; set; }
        public string? EventType { get; set; } // Ex: "Validation", "Achat", "Remboursement"
        public string? Description { get; set; } // Ex: "Café Philo" ou "Achat : Boost XP"
        public string UserName { get; set; } = string.Empty; // Le joueur concerné

        public int XpChange { get; set; } = 0;
        public int DocPointsChange { get; set; } = 0;
    }
}