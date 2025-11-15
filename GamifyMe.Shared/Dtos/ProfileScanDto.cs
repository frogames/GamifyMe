namespace GamifyMe.Shared.Dtos
{
    public class ProfileScanDto
    {
        public UserProfileDto UserProfile { get; set; } = null!;
        public List<PendingOrderDto> PendingOrders { get; set; } = new();
    }
}