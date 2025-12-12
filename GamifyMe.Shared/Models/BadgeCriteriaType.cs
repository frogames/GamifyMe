namespace GamifyMe.Shared.Models
{
    public enum BadgeCriteriaType
    {
        LevelReached,
        TotalXpEarned,
        TotalCurrencyEarned,
        ObjectivesCompletedCount, // Total number of validations
        ObjectiveSpecificCount, // e.g. "Validated 'Pushups' 10 times" - Wait, "ObjectiveSpecificCount" might be what I want? 
        // No, current ObjectiveSpecificCount uses ONE objective ID in CriteriaValue. I need MULTIPLE.
        // Let's add the requested ones.
        ObjectivesCompletedSelected, // Validated specific set of objectives
        StreakLength, // Best streak length
        StoreItemsPurchasedCount, // Shopaholic
        StoreItemsPurchasedSelected, // Purchased specific set of items
        Manual, // Given by staff manually
        OnboardingCompleted // Completed user onboarding
    }
}
