namespace GamifyMe.Shared.Dtos
{
    public class PlayerObjectiveDto
    {
        public Guid Id { get; set; } // Garder l'ID peut être utile
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int XpReward { get; set; }
        public int DocPointsReward { get; set; }
        public DateTime? EventDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
    }
}