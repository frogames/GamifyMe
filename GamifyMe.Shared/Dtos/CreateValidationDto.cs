namespace GamifyMe.Shared.Dtos
{
    public class CreateValidationDto
    {
        public string ScannedQrCodeContent { get; set; } = string.Empty;
        public Guid ObjectiveId { get; set; }
    }
}