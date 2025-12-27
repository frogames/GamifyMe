namespace GamifyMe.Shared.Models
{
    public enum EngagementEngine
    {
        Mastery,
        Social,
        Status
    }

    public enum FeedbackFrequency
    {
        Immediate,
        Deferred
    }

    public enum CycleType
    {
        Unique, // 1 time
        Repetitive, // x times
        Infinite // 0
    }
}
