namespace GamifyMe.Shared.Dtos
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string QrCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string EstablishmentName { get; set; } = string.Empty;
    }
}