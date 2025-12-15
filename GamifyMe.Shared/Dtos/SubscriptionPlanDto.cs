namespace GamifyMe.Shared.Dtos
{
    public class SubscriptionPlanDto
    {
        public string Id { get; set; } = string.Empty; // "free", "standard", "corporate"
        public string Name { get; set; } = string.Empty;
        public decimal PriceMonthly { get; set; }
        public decimal PriceYearly { get; set; }
        public int MaxUsers { get; set; }
        public List<string> Features { get; set; } = new();
        public bool IsCurrent { get; set; }
    }

    public class CurrentSubscriptionDto
    {
        public string PlanId { get; set; } = "free";
        public string Status { get; set; } = "active"; // active, past_due, canceled, trialing
        public DateTime? CurrentPeriodEnd { get; set; }
        public bool CancelAtPeriodEnd { get; set; }
        public string? Interval { get; set; } = "month"; // month, year
    }
}
