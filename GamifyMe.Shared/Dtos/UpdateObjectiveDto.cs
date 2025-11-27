namespace GamifyMe.Shared.Dtos
{
    public class UpdateObjectiveDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int XpReward { get; set; }
        public int DocPointsReward { get; set; }
        public bool IsActive { get; set; } // Pour pouvoir désactiver un objectif
        public bool IsUnique { get; set; }
        public DateTime? EventDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DisplayStartDate { get; set; }
        public DateTime? DisplayEndDate { get; set; }
        public int? FrequencyHours { get; set; }
        public string Location { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
    }
}