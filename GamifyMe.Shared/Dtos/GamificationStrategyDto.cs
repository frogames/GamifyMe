using GamifyMe.Shared.Models;

namespace GamifyMe.Shared.Dtos
{
    public class GamificationStrategyDto
    {
        public EngagementEngine EngagementEngine { get; set; }
        public FeedbackFrequency FeedbackFrequency { get; set; }
        public CycleType CycleType { get; set; }
        public int CycleDurationMonths { get; set; }
        public KitCategory ActivityDomain { get; set; }
    }
}
