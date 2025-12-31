namespace GamifyMe.Shared.Dtos
{
    public class KitImportResultDto
    {
        public int ObjectivesCount { get; set; }
        public int BadgesCount { get; set; }
        public int GroupsCount { get; set; }
        public int StoreItemsCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
