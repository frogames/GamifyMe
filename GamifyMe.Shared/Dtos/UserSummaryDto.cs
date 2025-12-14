namespace GamifyMe.Shared.Dtos
{
    public class UserSummaryDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public string Status { get; set; } = "active";
        public int XpBalance { get; set; }
        public int CurrencyBalance { get; set; }
    }
}
