namespace GamifyMe.Shared.Dtos
{
    public class PendingOrderDto
    {
        public Guid OrderId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemIcon { get; set; } = string.Empty;
        public DateTime DatePurchased { get; set; }
    }
}