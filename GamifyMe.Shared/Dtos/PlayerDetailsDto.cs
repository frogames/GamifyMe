namespace GamifyMe.Shared.Dtos
{
    public class PlayerDetailsDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int TotalXp { get; set; }
        public int TotalCurrency { get; set; }
        public int ActiveBoostMultiplier { get; set; } = 1;
        public DateTime? BoostEndsAt { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public Guid? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? GroupIcon { get; set; }
        public string? GroupColor { get; set; }
        public string? GroupImageUrl { get; set; }
        public List<PlayerObjectiveStreakDto> PrincipalStreaks { get; set; } = new();
        public List<BadgeDto> Badges { get; set; } = new();
    }

    public class PlayerObjectiveStreakDto
    {
        public string ObjectiveTitle { get; set; } = string.Empty;
        public int CurrentStreak { get; set; }
        public string? IconName { get; set; }
    }
}
