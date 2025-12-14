namespace GamifyMe.Shared.Dtos
{
    public class EstablishmentSettingsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = "Crédits";
        public int ArchiveUsersAfterInactiveDays { get; set; }
        public int MaxUsers { get; set; }
        
        public bool IsShopEnabled { get; set; }
        public bool IsGroupsEnabled { get; set; }
        public bool IsChallengesEnabled { get; set; }
    }
}
