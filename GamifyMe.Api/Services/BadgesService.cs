using GamifyMe.Api.Data;
using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Models;
using GamifyMe.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GamifyMe.Api.Services
{
    public class BadgesService
    {
        private readonly DataContext _context;
        private readonly ObjectiveService _objectiveService;

        public BadgesService(DataContext context, ObjectiveService objectiveService)
        {
            _context = context;
            _objectiveService = objectiveService;
        }

        public async Task<List<BadgeDto>> GetAllBadgesAsync(Guid userId)
        {
            var badges = await _context.Badges
                .AsNoTracking()
                .ToListAsync();

            var unlockedBadges = await _context.UserBadges
                .Where(ub => ub.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            // Fetch contextual data for progress calculation
            Wallet? xpWallet = null;
            Wallet? currencyWallet = null;
            Dictionary<Guid, int>? validationCounts = null;
            int level = 0;

            if (userId != Guid.Empty)
            {
                xpWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode == "XP");
                currencyWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode != "XP");
                int currentXp = (int)(xpWallet?.Balance ?? 0);
                level = LevelHelpers.GetLevelFromXp(currentXp);

                validationCounts = await _context.Validations
                    .Where(v => v.UserId == userId)
                    .GroupBy(v => v.ObjectiveId)
                    .Select(g => new { ObjectiveId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.ObjectiveId, x => x.Count);
            }

            // Fetch additional stats: Order Count & Active Objectives (for Streaks)
            int orderCount = 0;
            List<ObjectiveDto>? activeObjectives = null;
            if (userId != Guid.Empty)
            {
                orderCount = await _context.Orders.CountAsync(o => o.UserId == userId && o.Status == OrderStatus.Completed);
                
                // Fetch active objectives for streaks. 
                // We iterate over distinct establishments found in badges to cover all bases.
                var establishmentIds = badges.Select(b => b.EstablishmentId).Distinct().ToList();
                activeObjectives = new List<ObjectiveDto>();
                foreach(var estId in establishmentIds)
                {
                    var objs = await _objectiveService.GetActiveObjectivesAsync(userId, estId);
                    activeObjectives.AddRange(objs);
                }
            }

            // Fetch user inventory for "StoreItemsPurchasedSelected" progress
            HashSet<Guid>? ownedItemIds = null;
            if (userId != Guid.Empty)
            {
                 ownedItemIds = (await _context.UserInventories
                    .Where(ui => ui.UserId == userId)
                    .Select(ui => ui.StoreItemId)
                    .Distinct()
                    .ToListAsync())
                    .ToHashSet();
            }

            // Fetch reference data for rich details (Objectives & StoreItems)
            var allObjectives = await _context.Objectives.AsNoTracking().ToListAsync();
            var objectivesMap = allObjectives.ToDictionary(o => o.Id, o => MapObjectiveToDto(o));

            var allStoreItems = await _context.StoreItems.AsNoTracking().ToListAsync();
            var storeItemsMap = allStoreItems.ToDictionary(i => i.Id, i => MapStoreItemToDto(i));

            return badges.Select(b => {
                var ub = unlockedBadges.FirstOrDefault(x => x.BadgeId == b.Id);
                return MapToDto(b, ub, xpWallet, currencyWallet, validationCounts, level, activeObjectives, storeItemsMap, objectivesMap, ownedItemIds, orderCount);
            }).OrderByDescending(b => b.IsUnlocked).ThenBy(b => b.Category).ThenBy(b => b.Name).ToList();
        }

        public async Task<List<BadgeDto>> GetUnlockedBadgesAsync(Guid userId)
        {
            var userBadges = await _context.UserBadges
                .Include(ub => ub.Badge)
                .Where(ub => ub.UserId == userId)
                .OrderByDescending(ub => ub.UnlockedAt)
                .AsNoTracking()
                .ToListAsync();

            // We need reference data here too if we want full details in the popup
             var allObjectives = await _context.Objectives.AsNoTracking().ToListAsync();
            var objectivesMap = allObjectives.ToDictionary(o => o.Id, o => MapObjectiveToDto(o));

            var allStoreItems = await _context.StoreItems.AsNoTracking().ToListAsync();
            var storeItemsMap = allStoreItems.ToDictionary(i => i.Id, i => MapStoreItemToDto(i));

            return userBadges.Select(ub => MapToDto(ub.Badge, ub, null, null, null, 0, null, storeItemsMap, objectivesMap, null, 0)).ToList();
        }

        // --- CRUD ---

        public async Task<BadgeDto> CreateBadgeAsync(CreateBadgeDto request, Guid establishmentId)
        {
            var badge = new Badge
            {
                Id = Guid.NewGuid(),
                EstablishmentId = establishmentId,
                Name = request.Name,
                Category = request.Category,
                Description = request.Description,
                IconName = request.IconName,
                ImageUrl = request.ImageUrl,
                Color = request.Color,
                CriteriaType = request.CriteriaType,
                CriteriaValue = request.CriteriaValue,
                CriteriaThreshold = request.CriteriaThreshold,
                XpReward = request.XpReward,
                DocPointsReward = request.DocPointsReward,
                RewardStoreItemId = request.RewardStoreItemId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Badges.Add(badge);
            await _context.SaveChangesAsync();

            return MapToDto(badge, null, null, null, null, 0, null, null, null, null, 0);
        }

        public async Task<bool> UpdateBadgeAsync(Guid id, CreateBadgeDto request)
        {
            var badge = await _context.Badges.FindAsync(id);
            if (badge == null) return false;

            badge.Name = request.Name;
            badge.Category = request.Category;
            badge.Description = request.Description;
            badge.IconName = request.IconName;
            badge.ImageUrl = request.ImageUrl;
            badge.Color = request.Color;
            badge.CriteriaType = request.CriteriaType;
            badge.CriteriaValue = request.CriteriaValue;
            badge.CriteriaThreshold = request.CriteriaThreshold;
            badge.XpReward = request.XpReward;
            badge.DocPointsReward = request.DocPointsReward;
            badge.RewardStoreItemId = request.RewardStoreItemId;
            // Creation date untouched

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBadgeAsync(Guid id)
        {
            var badge = await _context.Badges.FindAsync(id);
            if (badge == null) return false;

            _context.Badges.Remove(badge);
            await _context.SaveChangesAsync();
            return true;
        }


        // --- UNLOCK LOGIC ---

        public async Task<List<BadgeDto>> CheckAndUnlockBadgesAsync(Guid userId, Guid establishmentId)
        {
            var now = DateTime.UtcNow;
            var unlockedNow = new List<BadgeDto>();

            // 1. Fetch User Data needed for evaluation
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return unlockedNow;

            // Load Wallets (XP/Currency)
            var xpWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode == "XP");
            var currencyWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode != "XP");
            int currentXp = (int)(xpWallet?.Balance ?? 0);
            int currentCurrency = (int)(currencyWallet?.Balance ?? 0);
            int level = LevelHelpers.GetLevelFromXp(currentXp);
            bool hasCompletedOnboarding = user.HasCompletedOnboarding;

            // Load Stats (Validation Counts)
            // Group Validations by ObjectiveId
            var validationCounts = await _context.Validations
                .Where(v => v.UserId == userId)
                .GroupBy(v => v.ObjectiveId)
                .Select(g => new { ObjectiveId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ObjectiveId, x => x.Count);

            int totalValidations = validationCounts.Sum(x => x.Value);

             // Load Streaks?
             // Only if we have Streak badges. This is expensive, so maybe do it only if needed?
             // Actually, ObjectiveService has streak logic.
             // We will load active objectives for streak checks here.
             var activeObjectives = await _objectiveService.GetActiveObjectivesAsync(userId, establishmentId);

            // 2. Fetch Badges NOT yet unlocked by user
            // We can't easily join on UserBadges in memory if list is huge, but here it's fine.
            // Or use SQL 'Where not exists'.
            var unlockedBadgeIds = await _context.UserBadges.Where(ub => ub.UserId == userId).Select(ub => ub.BadgeId).ToListAsync();
            
            var candidates = await _context.Badges
                .Where(b => b.EstablishmentId == establishmentId)
                .ToListAsync();

            var lockedCandidates = candidates.Where(b => !unlockedBadgeIds.Contains(b.Id)).ToList();
            if (!lockedCandidates.Any()) return unlockedNow;

            // 3. Evaluate Criteria

            foreach (var badge in lockedCandidates)
            {
                bool conditionsMet = false;

                switch (badge.CriteriaType)
                {
                    case BadgeCriteriaType.LevelReached:
                        conditionsMet = level >= badge.CriteriaThreshold;
                        break;
                    case BadgeCriteriaType.TotalXpEarned:
                        conditionsMet = currentXp >= badge.CriteriaThreshold;
                        break;
                    case BadgeCriteriaType.TotalCurrencyEarned:
                        // Careful: this checks CURRENT balance, not lifetime earned.
                        // Assuming this is intended for "Rich" status.
                        conditionsMet = currentCurrency >= badge.CriteriaThreshold;
                        break;
                    case BadgeCriteriaType.ObjectivesCompletedCount:
                        conditionsMet = totalValidations >= badge.CriteriaThreshold;
                        break;
                    case BadgeCriteriaType.ObjectiveSpecificCount:
                        // CriteriaValue should contain ObjectiveId
                        if (Guid.TryParse(badge.CriteriaValue, out Guid targetObjId))
                        {
                            if (validationCounts.TryGetValue(targetObjId, out int count))
                            {
                                conditionsMet = count >= badge.CriteriaThreshold;
                            }
                        }
                        break;
                    case BadgeCriteriaType.ObjectivesCompletedSelected:
                         if (!string.IsNullOrEmpty(badge.CriteriaValue))
                         {
                             try 
                             {
                                 var requiredObjIds = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(badge.CriteriaValue);
                                 if (requiredObjIds != null && requiredObjIds.Any())
                                 {
                                     // Check if ALL required objectives have at least one validation
                                     if (requiredObjIds.All(rid => validationCounts.ContainsKey(rid) && validationCounts[rid] > 0))
                                     {
                                         conditionsMet = true;
                                     }
                                 }
                             }
                             catch {}
                         }
                         break;
                    case BadgeCriteriaType.StoreItemsPurchasedCount:
                         // Simple count of orders
                         var orderCount = await _context.Orders.CountAsync(o => o.UserId == userId && o.Status == OrderStatus.Completed);
                         conditionsMet = orderCount >= badge.CriteriaThreshold;
                         break;
                    case BadgeCriteriaType.StoreItemsPurchasedSelected:
                         if (!string.IsNullOrEmpty(badge.CriteriaValue))
                         {
                             try
                             {
                                 var requiredItemIds = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(badge.CriteriaValue);
                                 if (requiredItemIds != null && requiredItemIds.Any())
                                 {
                                     // Check if user has purchased ALL these items
                                     // We check UserInventory for these ItemIds
                                     var userInventoryItemIds = await _context.UserInventories
                                         .Where(ui => ui.UserId == userId)
                                         .Select(ui => ui.StoreItemId)
                                         .Distinct()
                                         .ToListAsync();

                                     if (requiredItemIds.All(rid => userInventoryItemIds.Contains(rid)))
                                     {
                                         conditionsMet = true;
                                     }
                                 }
                             }
                             catch {}
                         }
                         break;
                    case BadgeCriteriaType.StreakLength:
                         // Check active streaks
                         // Check if ANY active objective has current streak >= Threshold
                         // Or if CriteriaValue specifies a specific objective
                         if (!string.IsNullOrEmpty(badge.CriteriaValue) && Guid.TryParse(badge.CriteriaValue, out Guid streakObjId))
                         {
                             var obj = activeObjectives.FirstOrDefault(o => o.Id == streakObjId);
                             if (obj != null && obj.CurrentStreak >= badge.CriteriaThreshold) conditionsMet = true;
                         }
                         else
                         {
                             // Any streak
                             if (activeObjectives.Any(o => o.CurrentStreak >= badge.CriteriaThreshold)) conditionsMet = true;
                         }
                         break;
                    case BadgeCriteriaType.TopOneIndividual:
                        // Check if user has the highest XP in the establishment
                        var bestIndividualXp = await _context.Wallets
                            .Where(w => w.CurrencyCode == "XP" && w.User.EstablishmentId == establishmentId)
                            .MaxAsync(w => (int?)w.Balance) ?? 0;
                        
                        conditionsMet = currentXp > 0 && currentXp >= bestIndividualXp;
                        break;
                    case BadgeCriteriaType.TopOneGroup:
                        if (user.GroupId.HasValue)
                        {
                            // Calculate group XPs
                            // Note: This is heavy, optimization would be to cache leaderboard or store group XP in Group table
                            var groupXps = await _context.Groups
                                .Where(g => g.EstablishmentId == establishmentId)
                                .Select(g => new 
                                { 
                                    g.Id, 
                                    TotalXp = g.Members.SelectMany(m => m.Wallets).Where(w => w.CurrencyCode == "XP").Sum(w => w.Balance) 
                                })
                                .OrderByDescending(g => g.TotalXp)
                                .FirstOrDefaultAsync();

                            if (groupXps != null && groupXps.Id == user.GroupId)
                            {
                                conditionsMet = true;
                            }
                        }
                        break;
                    case BadgeCriteriaType.OnboardingCompleted:
                        // User reported issue: badge given even if HasCompletedOnboarding=false.
                        // Ensure we strictly check the boolean.
                        conditionsMet = user.HasCompletedOnboarding == true;
                        break;
                }

                if (conditionsMet)
                {
                    await UnlockBadge(userId, badge, xpWallet, currencyWallet);
                    // Pass null maps here for CheckAndUnlock result, as this is just a quick check.
                    unlockedNow.Add(MapToDto(badge, new UserBadge { UnlockedAt = now, IsFavorite = false }, xpWallet, currencyWallet, validationCounts, level, null, null, null, null, 0));
                }
            }

            return unlockedNow;
        }

        private async Task UnlockBadge(Guid userId, Badge badge, Wallet? xpWallet, Wallet? currencyWallet)
        {
            // 1. Create UserBadge
            var userBadge = new UserBadge
            {
                Id = Guid.NewGuid(),
                EstablishmentId = badge.EstablishmentId,
                UserId = userId,
                BadgeId = badge.Id,
                UnlockedAt = DateTime.UtcNow,
                IsFavorite = false
            };
            _context.UserBadges.Add(userBadge);

            // 2. Give Rewards
            if (badge.XpReward > 0 && xpWallet != null)
            {
                xpWallet.Balance += badge.XpReward;
            }
            if (badge.DocPointsReward > 0 && currencyWallet != null)
            {
                currencyWallet.Balance += badge.DocPointsReward;
            }
            if (badge.RewardStoreItemId.HasValue)
            {
                // Give item (create Order/Inventory)
                var item = await _context.StoreItems.FindAsync(badge.RewardStoreItemId.Value);
                if (item != null)
                {
                    var inventory = new UserInventory
                    {
                        Id = Guid.NewGuid(),
                        EstablishmentId = badge.EstablishmentId,
                        UserId = userId,
                        StoreItemId = item.Id,
                        DateAcquired = DateTime.UtcNow,
                        IsActive = false
                    };
                    _context.UserInventories.Add(inventory);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> SetFavoriteBadgeAsync(Guid userId, Guid badgeId)
        {
            var userBadges = await _context.UserBadges.Where(ub => ub.UserId == userId).ToListAsync();
            var target = userBadges.FirstOrDefault(ub => ub.BadgeId == badgeId);
            
            if (target == null) return false; // User doesn't have this badge

            // Unset others? Requirement: "Un joueur peut choisir son badge préféré" (singular)
            // Implementation: Set IsFavorite=false for all, then true for target.
            foreach (var ub in userBadges) ub.IsFavorite = false;
            
            target.IsFavorite = true;
            await _context.SaveChangesAsync();
            return true;
        }

        private static BadgeDto MapToDto(
            Badge b, 
            UserBadge? ub,
            Wallet? xpWallet,
            Wallet? currencyWallet,
            Dictionary<Guid, int>? validationCounts,
            int currentLevel,
            List<ObjectiveDto>? activeObjectives,
            Dictionary<Guid, StoreItemDto>? storeItemsMap,
            Dictionary<Guid, ObjectiveDto>? objectivesMap,
            HashSet<Guid>? ownedItemIds,
            int orderCount)
        {
            double progress = 0;
            if (ub == null) // Only calculate progress if locked.
            {
                // Simple calc based on criteria
                try
                {
                    switch(b.CriteriaType)
                    {
                        case BadgeCriteriaType.LevelReached:
                            progress = currentLevel;
                            break;
                        case BadgeCriteriaType.TotalXpEarned:
                            progress = xpWallet?.Balance ?? 0;
                            break;
                        case BadgeCriteriaType.TotalCurrencyEarned:
                            progress = currencyWallet?.Balance ?? 0;
                            break;
                        case BadgeCriteriaType.ObjectivesCompletedCount:
                            progress = validationCounts?.Sum(x => x.Value) ?? 0;
                            break;
                        case BadgeCriteriaType.ObjectiveSpecificCount:
                            if (Guid.TryParse(b.CriteriaValue, out Guid tid) && validationCounts != null && validationCounts.TryGetValue(tid, out int c))
                                progress = c;
                            break;
                         case BadgeCriteriaType.ObjectivesCompletedSelected:
                            if (!string.IsNullOrEmpty(b.CriteriaValue) && validationCounts != null)
                            {
                                try
                                {
                                    var ids = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(b.CriteriaValue);
                                    if (ids != null)
                                    {
                                        b.CriteriaThreshold = ids.Count;
                                        // Count how many of the required objectives have been validated at least once
                                        progress = ids.Count(id => validationCounts.ContainsKey(id) && validationCounts[id] > 0);
                                    }
                                }
                                catch {}
                            }
                            break;
                         case BadgeCriteriaType.StoreItemsPurchasedSelected:
                            if (!string.IsNullOrEmpty(b.CriteriaValue) && ownedItemIds != null)
                            {
                                try
                                {
                                    var ids = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(b.CriteriaValue);
                                    if (ids != null)
                                    {
                                        b.CriteriaThreshold = ids.Count;
                                        // Count how many of the required items are owned
                                        progress = ids.Count(id => ownedItemIds.Contains(id));
                                    }
                                }
                                catch {}
                            }
                            break;
                        case BadgeCriteriaType.StoreItemsPurchasedCount:
                            progress = orderCount;
                            break;
                        case BadgeCriteriaType.StreakLength:
                             if (activeObjectives != null)
                             {
                                 // Check if CriteriaValue specifies a specific objective
                                 if (!string.IsNullOrEmpty(b.CriteriaValue) && Guid.TryParse(b.CriteriaValue, out Guid streakObjId))
                                 {
                                     var obj = activeObjectives.FirstOrDefault(o => o.Id == streakObjId);
                                     if (obj != null) progress = obj.CurrentStreak;
                                 }
                                 else
                                 {
                                     // Any streak -> Max streak among all
                                     if (activeObjectives.Any())
                                         progress = activeObjectives.Max(o => o.CurrentStreak);
                                 }
                             }
                             break;
                        default:
                            break;
                    }
                }
                catch {}
            }
            else
            {
                // If unlocked, progress is 100% (or max value)
                progress = b.CriteriaThreshold; 
            }

            // Populate rich data fields
            string? rewardItemName = null;
            StoreItemDto? rewardItem = null;
            if (b.RewardStoreItemId.HasValue && storeItemsMap != null && storeItemsMap.ContainsKey(b.RewardStoreItemId.Value))
            {
                rewardItem = storeItemsMap[b.RewardStoreItemId.Value];
                rewardItemName = rewardItem.Name;
            }



            List<ObjectiveDto>? requiredObjectives = null;
            if (b.CriteriaType == BadgeCriteriaType.ObjectivesCompletedSelected && !string.IsNullOrEmpty(b.CriteriaValue) && objectivesMap != null)
            {
                try
                {
                    var ids = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(b.CriteriaValue);
                    if (ids != null)
                    {
                        requiredObjectives = ids
                            .Where(id => objectivesMap.ContainsKey(id))
                            .Select(id => 
                            {
                                var obj = objectivesMap[id];
                                // Check status
                                obj.IsAlreadyCompleted = validationCounts != null && validationCounts.ContainsKey(id) && validationCounts[id] > 0;
                                return obj;
                            })
                            .ToList();
                    }
                }
                catch { }
            }

            List<StoreItemDto>? requiredStoreItems = null; 
            if (b.CriteriaType == BadgeCriteriaType.StoreItemsPurchasedSelected && !string.IsNullOrEmpty(b.CriteriaValue) && storeItemsMap != null)
            {
                try
                {
                    var ids = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(b.CriteriaValue);
                    if (ids != null)
                    {
                        requiredStoreItems = ids
                            .Where(id => storeItemsMap.ContainsKey(id))
                            .Select(id => 
                            {
                                var item = storeItemsMap[id];
                                // Check status
                                item.IsOwned = ownedItemIds != null && ownedItemIds.Contains(id);
                                return item;
                            })
                            .ToList();
                    }
                }
                catch { }
            }

            string? targetObjectiveName = null;
            if (objectivesMap != null && Guid.TryParse(b.CriteriaValue, out Guid tObjId))
            {
                // For Specific Count OR Streak Length (if specific objective is selected)
                if ((b.CriteriaType == BadgeCriteriaType.ObjectiveSpecificCount || b.CriteriaType == BadgeCriteriaType.StreakLength) && objectivesMap.ContainsKey(tObjId))
                {
                    targetObjectiveName = objectivesMap[tObjId].Title;
                }
            }

            return new BadgeDto
            {
                Id = b.Id,
                Name = b.Name,
                Category = b.Category,
                Description = b.Description,
                IconName = b.IconName,
                ImageUrl = b.ImageUrl,
                Color = b.Color,
                CriteriaType = b.CriteriaType,
                CriteriaValue = b.CriteriaValue,
                CriteriaThreshold = b.CriteriaThreshold,
                XpReward = b.XpReward,
                DocPointsReward = b.DocPointsReward,
                RewardStoreItemId = b.RewardStoreItemId,
                IsUnlocked = ub != null,
                UnlockedAt = ub?.UnlockedAt,
                IsFavorite = ub?.IsFavorite ?? false,
                CurrentValue = progress,
                RewardStoreItemName = rewardItemName,
                RewardStoreItem = rewardItem,
                RequiredObjectives = requiredObjectives,
                RequiredStoreItems = requiredStoreItems,
                TargetObjectiveName = targetObjectiveName
            };
        }

        private static StoreItemDto MapStoreItemToDto(StoreItem item)
        {
            return new StoreItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                IconName = item.IconName,
                Price = item.Price,
                Stock = item.Stock,
                ItemType = item.ItemType,
                IsActive = item.IsActive,
                ImageUrl = item.ImageUrl,
                Color = item.Color,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                SortOrder = item.SortOrder,
                DigitalActionCode = item.DigitalActionCode,
                DigitalAssetUrl = item.DigitalAssetUrl,
                IsUnique = item.IsUnique,
                IsOwned = false, // Not context relevant here
                CreatedAt = item.CreatedAt
            };
        }

        private static ObjectiveDto MapObjectiveToDto(Objective obj)
        {
             return new ObjectiveDto
            {
                Id = obj.Id,
                Title = obj.Title,
                Description = obj.Description,
                IconName = obj.IconName,
                Color = obj.Color,
                XpReward = obj.XpReward,
                DocPointsReward = obj.DocPointsReward,
                Location = obj.Location,
                EventDate = obj.EventDate,
                EndDate = obj.EndDate,
                DisplayStartDate = obj.DisplayStartDate,
                DisplayEndDate = obj.DisplayEndDate,
                IsUnique = obj.IsUnique,
                FrequencyHours = obj.FrequencyHours,
                IsActive = obj.IsActive,
                LifespanHours = obj.LifespanHours,
                Category = obj.Category,
                SortOrder = obj.SortOrder,
                CreatedAt = obj.CreatedAt,
                IsStreakEnabled = obj.IsStreakEnabled,
                StreakTerminalHours = obj.StreakTerminalHours,
                StreakFrequency = obj.StreakFrequency,
                StreakExcludedDays = obj.StreakExcludedDays,
                StreakExcludedMonths = obj.StreakExcludedMonths
            };
        }

    }
}
