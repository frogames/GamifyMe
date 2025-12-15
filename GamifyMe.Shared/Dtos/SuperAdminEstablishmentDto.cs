namespace GamifyMe.Shared.Dtos
{
    public class SuperAdminEstablishmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public int MaxUsers { get; set; }
        public bool IsTemplate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
