namespace GamifyMe.Shared.Dtos
{
    public class ValidationHistoryDto
    {
        public DateTime Date { get; set; }
        public string ObjectiveTitle { get; set; } = string.Empty;
        public int XpGained { get; set; } // Bonus : on peut aussi montrer les points gagnés
        public int DocPointsGained { get; set; }
    }
}