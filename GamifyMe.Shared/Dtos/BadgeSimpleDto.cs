namespace GamifyMe.Shared.Dtos
{
    public class BadgeSimpleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconName { get; set; }
        public string? ImageUrl { get; set; }
        public string? Color { get; set; }
    }
}
