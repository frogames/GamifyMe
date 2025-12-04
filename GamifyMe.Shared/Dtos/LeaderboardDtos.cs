namespace GamifyMe.Shared.Dtos
{
    public class UserLeaderboardEntryDto
    {
        public int Rank { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int TotalXp { get; set; }
        public int TotalCurrency { get; set; }
        public string? GroupName { get; set; }
    }

    public class GroupLeaderboardEntryDto
    {
        public int Rank { get; set; }
        public Guid GroupId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconName { get; set; }
        public string? Color { get; set; }
        public int MemberCount { get; set; }
        public int TotalXp { get; set; }
    }
}
