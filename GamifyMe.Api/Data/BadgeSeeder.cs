using GamifyMe.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GamifyMe.Api.Data
{
    public static class BadgeSeeder
    {
        public static async Task SeedAsync(DataContext context)
        {
            var establishments = await context.Establishments.IgnoreQueryFilters().ToListAsync();

            foreach (var est in establishments)
            {
                // We need to temporarily disable query filters to ensure we are acting on the specific establishment's scope
                // Actually, adding items explicitly setting EstablishmentId works fine even with filters enabled usually, 
                // but reading 'Any' might be affected if we are in a context with a user.
                // Here we are likely in a scope without user (startup).
                
                // Check Level Badges 2 to 9 (User requested restoration, 0 rewards, deleted Level 1)
                for (int i = 2; i <= 9; i++)
                {
                    string badgeName = $"Niveau {i}";
                    
                    // Check existence (globally or for this establishment)
                    // Since we are iterating, we check for THIS establishment.
                    bool exists = await context.Badges.IgnoreQueryFilters()
                        .AnyAsync(b => b.EstablishmentId == est.Id && b.Name == badgeName && b.CriteriaType == BadgeCriteriaType.LevelReached);
                    
                    if (!exists)
                    {
                        var badge = new Badge
                        {
                            Id = Guid.NewGuid(),
                            EstablishmentId = est.Id,
                            Name = badgeName,
                            Description = $"Atteindre le niveau {i}",
                            IconName = "Star", // Standard icon
                            Color = "#FFD700", // Gold
                            CriteriaType = BadgeCriteriaType.LevelReached,
                            CriteriaThreshold = i,
                            CriteriaValue = i.ToString(),
                            XpReward = 0, 
                            DocPointsReward = 0, 
                            RewardStoreItemId = null,
                            CreatedAt = DateTime.UtcNow
                        };
                        context.Badges.Add(badge);
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
