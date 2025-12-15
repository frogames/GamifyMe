using GamifyMe.Api.Data;
using GamifyMe.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GamifyMe.Api.Services
{
    public class ContentImportService
    {
        private readonly DataContext _context;

        public ContentImportService(DataContext context)
        {
            _context = context;
        }

        public async Task ImportKitAsync(Guid sourceEstablishmentId, Guid targetEstablishmentId)
        {
            // 1. Load Source Data
            var sourceObjectives = await _context.Objectives
                .Include(o => o.Prerequisites)
                .Where(o => o.EstablishmentId == sourceEstablishmentId)
                .AsNoTracking()
                .ToListAsync();
            var sourceStoreItems = await _context.StoreItems.Where(s => s.EstablishmentId == sourceEstablishmentId).AsNoTracking().ToListAsync();
            var sourceGroups = await _context.Groups.Where(g => g.EstablishmentId == sourceEstablishmentId).AsNoTracking().ToListAsync();
            var sourceBadges = await _context.Badges.Where(b => b.EstablishmentId == sourceEstablishmentId).AsNoTracking().ToListAsync();

            // ID Mappings: OldId -> NewId
            var objectiveMap = new Dictionary<Guid, Guid>();
            var newObjectiveEntities = new Dictionary<Guid, Objective>(); // Start tracking entities for relationship linking
            var storeItemMap = new Dictionary<Guid, Guid>();
            var groupMap = new Dictionary<Guid, Guid>(); 

            // 2. Clone Objectives
            foreach (var srcObj in sourceObjectives)
            {
                var newId = Guid.NewGuid();
                objectiveMap[srcObj.Id] = newId;

                var newObj = new Objective
                {
                    Id = newId,
                    EstablishmentId = targetEstablishmentId,
                    Title = srcObj.Title,
                    Description = srcObj.Description,
                    IconName = srcObj.IconName,
                    Color = srcObj.Color,
                    XpReward = srcObj.XpReward,
                    DocPointsReward = srcObj.DocPointsReward,
                    Location = srcObj.Location,
                    EventDate = srcObj.EventDate,
                    EndDate = srcObj.EndDate,
                    DisplayStartDate = srcObj.DisplayStartDate,
                    DisplayEndDate = srcObj.DisplayEndDate,
                    IsUnique = srcObj.IsUnique,
                    FrequencyHours = srcObj.FrequencyHours,
                    IsActive = false, 
                    LifespanHours = srcObj.LifespanHours,
                    Category = srcObj.Category,
                    SortOrder = srcObj.SortOrder,
                    CreatedAt = DateTime.UtcNow,
                    IsStreakEnabled = srcObj.IsStreakEnabled,
                    StreakTerminalHours = srcObj.StreakTerminalHours,
                    StreakFrequency = srcObj.StreakFrequency,
                    StreakExcludedDays = srcObj.StreakExcludedDays,
                    StreakExcludedMonths = srcObj.StreakExcludedMonths
                };
                
                newObjectiveEntities[srcObj.Id] = newObj;
                _context.Objectives.Add(newObj);
            }

            // 2.1 Link Prerequisites
            foreach (var srcObj in sourceObjectives)
            {
                if (srcObj.Prerequisites != null && srcObj.Prerequisites.Any())
                {
                    var newObj = newObjectiveEntities[srcObj.Id];
                    foreach (var oldPrereq in srcObj.Prerequisites)
                    {
                        if (newObjectiveEntities.ContainsKey(oldPrereq.Id))
                        {
                            newObj.Prerequisites.Add(newObjectiveEntities[oldPrereq.Id]);
                        }
                    }
                }
            }

            // 3. Clone Store Items
            foreach (var srcItem in sourceStoreItems)
            {
                var newId = Guid.NewGuid();
                storeItemMap[srcItem.Id] = newId;

                var newItem = new StoreItem
                {
                    Id = newId,
                    EstablishmentId = targetEstablishmentId,
                    Name = srcItem.Name,
                    Description = srcItem.Description,
                    IconName = srcItem.IconName,
                    Price = srcItem.Price,
                    Stock = srcItem.Stock,
                    ItemType = srcItem.ItemType,
                    IsActive = false, // User requested inactive by default
                    ImageUrl = srcItem.ImageUrl,
                    Color = srcItem.Color,
                    StartDate = srcItem.StartDate,
                    EndDate = srcItem.EndDate,
                    SortOrder = srcItem.SortOrder,
                    DigitalActionCode = srcItem.DigitalActionCode,
                    DigitalAssetUrl = srcItem.DigitalAssetUrl,
                    IsUnique = srcItem.IsUnique,
                    CreatedAt = DateTime.UtcNow
                };
                _context.StoreItems.Add(newItem);
            }

            // 4. Clone Groups
            foreach (var srcGroup in sourceGroups)
            {
                var newId = Guid.NewGuid();
                groupMap[srcGroup.Id] = newId;

                var newGroup = new Group
                {
                    Id = newId,
                    EstablishmentId = targetEstablishmentId,
                    Name = srcGroup.Name,
                    Description = srcGroup.Description,
                    IconName = srcGroup.IconName,
                    Color = srcGroup.Color,
                    IsActive = false, // User requested inactive by default
                    TotalXp = 0,
                    RegistrationDurationHours = srcGroup.RegistrationDurationHours,
                    ImageUrl = srcGroup.ImageUrl,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Groups.Add(newGroup);
            }

            // 5. Clone Badges (Complex: Remap IDs)
            foreach (var srcBadge in sourceBadges)
            {
                var newBadge = new Badge
                {
                    Id = Guid.NewGuid(),
                    EstablishmentId = targetEstablishmentId,
                    Name = srcBadge.Name,
                    Category = srcBadge.Category,
                    Description = srcBadge.Description,
                    IconName = srcBadge.IconName,
                    ImageUrl = srcBadge.ImageUrl,
                    Color = srcBadge.Color,
                    IsActive = false, // User requested inactive by default
                    CriteriaType = srcBadge.CriteriaType,
                    // CriteriaValue needs remapping!
                    CriteriaThreshold = srcBadge.CriteriaThreshold,
                    XpReward = srcBadge.XpReward,
                    DocPointsReward = srcBadge.DocPointsReward,
                    // RewardStoreItem needs remapping
                    CreatedAt = DateTime.UtcNow
                };

                // Remap Reward Item
                if (srcBadge.RewardStoreItemId.HasValue && storeItemMap.ContainsKey(srcBadge.RewardStoreItemId.Value))
                {
                    newBadge.RewardStoreItemId = storeItemMap[srcBadge.RewardStoreItemId.Value];
                }

                // Remap Criteria Value
                newBadge.CriteriaValue = RemapCriteriaValue(srcBadge.CriteriaType, srcBadge.CriteriaValue, objectiveMap, storeItemMap);

                _context.Badges.Add(newBadge);
            }

            await _context.SaveChangesAsync();
        }

        private string? RemapCriteriaValue(BadgeCriteriaType type, string? originalValue, Dictionary<Guid, Guid> objMap, Dictionary<Guid, Guid> itemMap)
        {
            if (string.IsNullOrEmpty(originalValue)) return originalValue;

            try
            {
                switch (type)
                {
                    case BadgeCriteriaType.ObjectiveSpecificCount:
                    case BadgeCriteriaType.StreakLength: // Can target specific objective
                        if (Guid.TryParse(originalValue, out Guid objId) && objMap.ContainsKey(objId))
                        {
                            return objMap[objId].ToString();
                        }
                        break;

                    case BadgeCriteriaType.ObjectivesCompletedSelected:
                        var objIds = JsonSerializer.Deserialize<List<Guid>>(originalValue);
                        if (objIds != null)
                        {
                            var newObjIds = objIds.Select(id => objMap.ContainsKey(id) ? objMap[id] : id).ToList(); // If not mapped (shouldn't happen), keep original? Or filter? Keeping original safe but broken.
                            return JsonSerializer.Serialize(newObjIds);
                        }
                        break;

                    case BadgeCriteriaType.StoreItemsPurchasedSelected:
                        var itemIds = JsonSerializer.Deserialize<List<Guid>>(originalValue);
                        if (itemIds != null)
                        {
                            var newItemIds = itemIds.Select(id => itemMap.ContainsKey(id) ? itemMap[id] : id).ToList();
                            return JsonSerializer.Serialize(newItemIds);
                        }
                        break;
                }
            }
            catch
            {
                // If parse fails or something weird, return original (safe fallback, though likely broken logic)
            }

            return originalValue;
        }
    }
}
