namespace GamifyMe.Shared.Dtos
{
    public class ValidationResponseDto
    {
        public string? Message { get; set; }
        public string? Username { get; set; }
        public int NewXpBalance { get; set; }
        public int NewDocPointsBalance { get; set; }
    }
}