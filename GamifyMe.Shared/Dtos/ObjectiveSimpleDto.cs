namespace GamifyMe.Shared.Dtos
{
    // DTO léger juste pour les listes de sélection
    public class ObjectiveSimpleDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public bool IsStreakEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}