using GamifyMe.Api.Data;
using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GamifyMe.Api.Services
{
    public class ObjectiveService
    {
        private readonly DataContext _context;

        public ObjectiveService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<ObjectiveDto>> GetActiveObjectivesAsync(Guid userId, Guid establishmentId)
        {
            var now = DateTime.UtcNow;

            // 1. Get user validations with timestamps
            var userValidations = await _context.Validations
                .Where(v => v.UserId == userId)
                .Select(v => new { v.ObjectiveId, ValidatedAt = v.Date })
                .ToListAsync();
            
            // Group by ObjectiveId and take the latest validation
            var lastValidationDates = userValidations
                .GroupBy(v => v.ObjectiveId)
                .ToDictionary(g => g.Key, g => g.Max(v => v.ValidatedAt));

            // 2. Get active objectives for the establishment
            var allActiveObjectives = await _context.Objectives
                .Include(o => o.Prerequisites)
                .Include(o => o.IsPrerequisiteFor)
                .Where(o => o.EstablishmentId == establishmentId
                            && o.IsActive
                            && (o.EndDate == null || o.EndDate > now)
                            && (o.DisplayStartDate == null || o.DisplayStartDate <= now)
                            && (o.DisplayEndDate == null || o.DisplayEndDate >= now))
                .AsNoTracking()
                .ToListAsync();

            // 3. Get Active Bonus Periods (XP and Credits)
            var activeBonuses = await _context.BonusPeriods
                .Where(b => b.EstablishmentId == establishmentId
                            && b.IsActive
                            && b.StartDate <= now
                            && b.EndDate >= now)
                .ToListAsync();

            // 4. Get User Active Boosts
            // Note: DigitalActionCode is typically UPPERCASE (e.g. BOOST_XP_2X)
            var userBoosts = await _context.UserInventories
                .Include(ui => ui.StoreItem)
                .Where(ui => ui.UserId == userId
                             && ui.IsActive
                             && (ui.ExpiresAt == null || ui.ExpiresAt > now)
                             && ui.StoreItem.ItemType == StoreItemType.Digital
                             && ui.StoreItem.DigitalActionCode != null
                             && (ui.StoreItem.DigitalActionCode.StartsWith("BOOST_XP_") 
                                 || ui.StoreItem.DigitalActionCode.StartsWith("XP_BOOST_"))) 
                .ToListAsync();

            double totalMultiplier = 1.0;
            var bonusLabels = new List<string>();

            foreach (var bonus in activeBonuses)
            {
                if (bonus.Type == BonusType.Xp)
                    totalMultiplier *= bonus.Multiplier;

                // Example: "Joyeux Noël (XP x2)" or "Pâques (Crédits x2)"
                var typeStr = bonus.Type == BonusType.Xp ? "XP" : "Crédits";
                bonusLabels.Add($"{bonus.Name} ({typeStr} x{bonus.Multiplier})");
            }

            foreach (var boost in userBoosts)
            {
                var code = boost.StoreItem.DigitalActionCode!;
                var parts = code.Split('_');
                
                // Try to find the multiplier part
                // It is usually the 3rd part (index 2) like in BOOST_XP_2X or XP_BOOST_2X_24H
                // We will iterate to find a part looking like a number
                double m = 0;
                bool found = false;

                foreach (var part in parts)
                {
                    var cleanPart = part.Replace("X", "", StringComparison.OrdinalIgnoreCase).Replace("x", "", StringComparison.OrdinalIgnoreCase);
                    if (double.TryParse(cleanPart,  System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
                    {
                        // Avoid grabbing dates/hours like "24H" -> 24 if we aren't careful? 
                        // Usually "24H" is parsed as 24? No, "24H" has 'H'. Replace only replaced 'X'.
                        // So "24H" wont parse as double unless we remove H.
                        // But what if the multiplier is "2"?
                        
                        // Heuristic: Multiplier is usually small (e.g. < 100).
                        if (val > 0 && val < 20) // Safety check
                        {
                            m = val;
                            found = true;
                            // We assume the first number we find (that is not part of explicit duration) is the multiplier
                            // Actually, "2X" is standard.
                            if (part.Contains("X", StringComparison.OrdinalIgnoreCase)) break; // Validated by 'X' presence
                        }
                    }
                }

                if (found)
                {
                    totalMultiplier *= m;
                    bonusLabels.Add($"{boost.StoreItem.Name} (XP x{m})");
                }
                else
                {
                    // Fallback if parsing failed but we know it's a boost
                    // We shouldn't apply random multiplier, but maybe show label?
                    // Better to rely on "found".
                }
            }

            // Filter out expired by Lifespan
            var validObjectives = new List<Objective>();
            foreach (var obj in allActiveObjectives)
            {
                if (obj.LifespanHours.HasValue)
                {
                    var startTime = obj.DisplayStartDate ?? obj.CreatedAt;
                    if (startTime.AddHours(obj.LifespanHours.Value) < now)
                    {
                        continue; // Expired
                    }
                }
                validObjectives.Add(obj);
            }

            // Fetch user status regarding onboarding
            bool hasCompletedOnboarding = false;
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null) hasCompletedOnboarding = user.HasCompletedOnboarding;

            // --- ONBOARDING LOGIC ---
            // If user has completed onboarding, we ignore onboarding objectives (they are considered "done" or "hidden")
            var onboardingObjectives = hasCompletedOnboarding 
                ? new List<Objective>() 
                : validObjectives.Where(o => o.Category == ObjectiveCategory.Onboarding).ToList();

            // All other non-onboarding objectives (Principal, Event, Secondary, etc.)
            var standardObjectives = validObjectives.Where(o => o.Category != ObjectiveCategory.Onboarding).ToList();

            Objective? currentOnboardingObjective = null;

            if (onboardingObjectives.Any())
            {
                // Find roots: Onboarding objectives that do not have any Prerequisite that IS ALSO an onboarding objective
                // (This allows them to have other prerequisites if needed, but not from the chain)
                var roots = new List<Objective>();
                foreach (var obj in onboardingObjectives)
                {
                    bool isRoot = true;
                    if (obj.Prerequisites != null && obj.Prerequisites.Any())
                    {
                         // Check if any prerequisite is an onboarding objective
                         if (obj.Prerequisites.Any(p => onboardingObjectives.Any(o => o.Id == p.Id)))
                         {
                             isRoot = false;
                         }
                    }
                    if (isRoot) roots.Add(obj);
                }

                foreach (var root in roots)
                {
                    var current = root;
                    while (current != null)
                    {
                        // Check if validated
                        if (!lastValidationDates.ContainsKey(current.Id))
                        {
                            // Found the first unvalidated step!
                            currentOnboardingObjective = current;
                            break;
                        }

                        // Move to next: Find a child in 'onboardingObjectives' that has 'current' as prerequisite
                        // relying on IsPrerequisiteFor
                        var next = current.IsPrerequisiteFor
                            ?.FirstOrDefault(child => onboardingObjectives.Any(o => o.Id == child.Id));

                        // We need the full object from our memory list
                        if (next != null)
                        {
                            current = onboardingObjectives.FirstOrDefault(o => o.Id == next.Id);
                        }
                        else
                        {
                            current = null; // End of chain
                        }
                    }
                    if (currentOnboardingObjective != null) break; // Found one
                }
            }

            if (currentOnboardingObjective != null)
            {
                // Onboarding Mode: Return ONLY this objective
                var dto = MapToDto(currentOnboardingObjective, false);
                dto.ActiveMultiplier = totalMultiplier;
                dto.BonusLabel = bonusLabels.Any() ? string.Join(", ", bonusLabels) : null;
                return new List<ObjectiveDto> { dto };
            }

            // --- STANDARD LOGIC ---

            // Build Chain Info (x/y)
            // We assume a linear chain for simplicity as requested ("single prerequisite")
            // But we must handle the graph structure safely.
            var objectiveDtos = new List<ObjectiveDto>();
            
            // Helper dictionary for quick lookup
            var objMap = standardObjectives.ToDictionary(o => o.Id);

            foreach (var obj in standardObjectives)
            {
                // Check prerequisites (Locking logic)
                bool isLocked = false;
                if (obj.Prerequisites != null && obj.Prerequisites.Any())
                {
                    // Check if ALL prerequisites are validated (at least once)
                    if (!obj.Prerequisites.All(p => lastValidationDates.ContainsKey(p.Id)))
                        isLocked = true;
                }
                if (isLocked) continue;

                // Check completion / cooldown
                bool hasValidated = lastValidationDates.TryGetValue(obj.Id, out var lastValidatedAt);
                bool isAlreadyCompleted = false;
                DateTime? nextAvailableDate = null;

                if (obj.IsUnique)
                {
                    if (hasValidated) continue; // Hide completed unique objectives
                }
                else
                {
                    // Frequency logic
                    if (hasValidated && obj.FrequencyHours.HasValue)
                    {
                        var cooldownEnd = lastValidatedAt.AddHours(obj.FrequencyHours.Value);
                        if (cooldownEnd > now)
                        {
                            nextAvailableDate = cooldownEnd;
                            isAlreadyCompleted = true; // Mark as "done for now" (grayed out)
                        }
                    }
                }

                // Calculate Chain Position
                int chainPos = 1;
                int chainLen = 1;
                
                // Traverse up (Prerequisites) to find position
                var current = obj;
                var visited = new HashSet<Guid> { current.Id };
                while (current.Prerequisites != null && current.Prerequisites.Any())
                {
                    // Take the first prerequisite (assuming single chain)
                    var parentId = current.Prerequisites.First().Id;
                    // We need to look up the parent in our loaded list to continue traversing
                    // Note: Parent might not be in 'standardObjectives' if it's expired/inactive, 
                    // but usually it should be. If not found, we stop.
                    var parent = allActiveObjectives.FirstOrDefault(o => o.Id == parentId);
                    if (parent == null || visited.Contains(parent.Id)) break;
                    
                    visited.Add(parent.Id);
                    current = parent;
                    chainPos++;
                }

                // Traverse down (IsPrerequisiteFor) to find total length
                // We start from the ROOT we found (current is now the root)
                // But wait, 'obj' is the current item. 
                // Total length = (depth of obj) + (max depth below obj) - 1?
                // Simpler: Find the root, then traverse down to find max depth.
                
                // Let's find the root first (already done, 'current' is root).
                // Now find max depth starting from 'current'.
                chainLen = GetMaxChainDepth(current, allActiveObjectives, new HashSet<Guid>());

                // Calculate Validation Count (All time)
                int validationCount = 0;
                var objectiveValidations = userValidations.Where(v => v.ObjectiveId == obj.Id).OrderByDescending(v => v.ValidatedAt).ToList();
                validationCount = objectiveValidations.Count;

                int currentStreak = 0;
                DateTime? streakExpiration = null;
                if (obj.IsStreakEnabled && !obj.IsUnique)
                {
                    if (objectiveValidations.Any())
                    {
                        var lastVal = objectiveValidations[0].ValidatedAt;
                        
                        // --- HOURLY LOGIC ---
                        if (obj.StreakFrequency == StreakFrequency.Hourly)
                        {
                            currentStreak = 1; 
                            double hoursSinceLast = (now - lastVal).TotalHours;
                            if (obj.StreakTerminalHours.HasValue && hoursSinceLast > obj.StreakTerminalHours.Value)
                            {
                                currentStreak = 0;
                            }
                            else
                            {
                                if (obj.StreakTerminalHours.HasValue)
                                    streakExpiration = lastVal.AddHours(obj.StreakTerminalHours.Value);

                                for (int i = 0; i < objectiveValidations.Count - 1; i++)
                                {
                                    var currentValTime = objectiveValidations[i].ValidatedAt;
                                    var prevValTime = objectiveValidations[i+1].ValidatedAt;
                                    var gap = (currentValTime - prevValTime).TotalHours;
                                    if (obj.StreakTerminalHours.HasValue && gap <= obj.StreakTerminalHours.Value)
                                        currentStreak++;
                                    else
                                        break;
                                }
                            }
                        }
                        // --- DAILY LOGIC ---
                        else if (obj.StreakFrequency == StreakFrequency.Daily)
                        {
                             // Simple Daily Logic:
                             // Streak is unbroken if last validation was Today OR "Yesterday (or last valid day)".
                             // If "Yesterday" was skipped but was an excluded day, we look further back.
                             
                             // 1. Check if "current" streak is alive (did we miss the deadline?)
                             // Deadline for Daily is usually "End of Today" (if not done today) or "End of Tomorrow" (if done today).
                             // Actually, if I did it TODAY, my streak is safe until TOMORROW end.
                             // If I did it YESTERDAY, my streak is safe until TODAY end.
                             // If I did it 2 days ago, and yesterday was REQUIRED, streak is dead.

                             // Parse Excluded Days
                             var excludedDays = new HashSet<DayOfWeek>();
                             if (!string.IsNullOrEmpty(obj.StreakExcludedDays))
                             {
                                 foreach(var s in obj.StreakExcludedDays.Split(','))
                                     if(int.TryParse(s, out int d)) excludedDays.Add((DayOfWeek)d);
                             }

                             // Is Streak Alive?
                             // We check if we missed any REQUIRED day between LastValidation.Date and Today.Date (exclusive of LastValidation, inclusive of Today if checked? No, Today is active opportunity).
                             // Actually, if validation is TODAY, streak satisfies today.
                             // If validation is YESTERDAY, streak is PENDING today.
                             // If validation < YESTERDAY, we check days in between.
                             
                             bool isStreakAlive = true;
                             var dateCursor = lastVal.Date.AddDays(1);
                             var today = now.Date;

                             while (dateCursor < today)
                             {
                                 if (!excludedDays.Contains(dateCursor.DayOfWeek))
                                 {
                                     isStreakAlive = false;
                                     break;
                                 }
                                 dateCursor = dateCursor.AddDays(1);
                             }

                             if (!isStreakAlive)
                             {
                                 currentStreak = 0;
                             }
                             else
                             {
                                 // Streak is alive! Now count history.
                                 currentStreak = 1;
                                 
                                 // We need to iterate backwards validations and ensure no gaps.
                                 // We can group validations by Date to avoid multi-validation per day issues.
                                 var distinctDates = objectiveValidations.Select(v => v.ValidatedAt.Date).Distinct().ToList(); // Sorted Desc
                                 
                                 for(int i = 0; i < distinctDates.Count - 1; i++)
                                 {
                                     var dCurrent = distinctDates[i];
                                     var dPrev = distinctDates[i+1];
                                     
                                     // Check gaps between dCurrent and dPrev
                                     bool gapOk = true;
                                     var checkDate = dPrev.AddDays(1);
                                     while(checkDate < dCurrent)
                                     {
                                          if (!excludedDays.Contains(checkDate.DayOfWeek))
                                          {
                                              gapOk = false; 
                                              break;
                                          }
                                          checkDate = checkDate.AddDays(1);
                                     }
                                     
                                     if (gapOk) currentStreak++;
                                     else break;
                                 }
                                 
                                 // Calculate Expiration: End of Next Required Day
                                 // If done today, next required is... distinctDates[0] is LastVal Date.
                                 // If LastVal == Today, we are good for today. Next deadline is End of "Next Required Day".
                                 // If LastVal < Today (and alive), deadline is End of Today (if required) or Next Required.
                                 
                                 var baseDateForNext = (lastVal.Date == today) ? today.AddDays(1) : today;
                                 // Find first required day starting from baseDateForNext
                                 while (excludedDays.Contains(baseDateForNext.DayOfWeek))
                                 {
                                     baseDateForNext = baseDateForNext.AddDays(1);
                                 }
                                 streakExpiration = baseDateForNext.AddDays(1).AddSeconds(-1); // End of that day
                             }
                        }
                        // --- WEEKLY/MONTHLY (Simplification: just basic period specific logic or fallback) ---
                        else
                        {
                            // Placeholder: Simple count
                            currentStreak = objectiveValidations.Count;
                        }
                    }
                }

                var dto = MapToDto(obj, isAlreadyCompleted);
                dto.NextAvailableDate = nextAvailableDate;
                dto.ActiveMultiplier = totalMultiplier;
                dto.BonusLabel = bonusLabels.Any() ? string.Join(", ", bonusLabels) : null;
                dto.ValidationCount = validationCount;
                dto.CurrentStreak = currentStreak;
                dto.StreakExpirationDate = streakExpiration;

                if (obj.LifespanHours.HasValue)
                {
                    var startTime = obj.DisplayStartDate ?? obj.CreatedAt;
                    dto.ExpirationDate = startTime.AddHours(obj.LifespanHours.Value);
                }
                
                if (chainLen > 1)
                {
                    // Title modification removed
                    dto.ChainPosition = chainPos;
                    dto.ChainLength = chainLen;
                }

                objectiveDtos.Add(dto);
            }

            return objectiveDtos.OrderBy(o => o.Category).ThenBy(o => o.SortOrder).ToList();
        }

        private int GetMaxChainDepth(Objective current, List<Objective> allObjectives, HashSet<Guid> visited)
        {
            if (visited.Contains(current.Id)) return 0; // Cycle detected
            visited.Add(current.Id);

            // Find children using the loaded navigation property
            // We must filter to only include those present in 'allObjectives' (active ones)
            var children = current.IsPrerequisiteFor
                .Where(child => allObjectives.Any(active => active.Id == child.Id))
                .ToList();
            
            if (!children.Any()) return 1;

            int maxChildDepth = 0;
            foreach (var childRef in children)
            {
                // VITAL FIX: We must use the instance from 'allObjectives' which has relations populated.
                // The 'childRef' from navigation property might not have its own 'IsPrerequisiteFor' loaded.
                var fullChild = allObjectives.First(o => o.Id == childRef.Id);

                int depth = GetMaxChainDepth(fullChild, allObjectives, new HashSet<Guid>(visited));
                if (depth > maxChildDepth) maxChildDepth = depth;
            }

            return 1 + maxChildDepth;
        }

        public async Task<List<ObjectiveDto>> GetAllObjectivesFullListAsync()
        {
            var objectives = await _context.Objectives
                .Include(o => o.Prerequisites)
                .OrderBy(o => o.Category)
                .ThenBy(o => o.SortOrder)
                .ToListAsync();

            return objectives.Select(o => MapToDto(o, false)).ToList();
        }

        public async Task<List<ObjectiveSimpleDto>> GetAllObjectivesSimpleListAsync()
        {
            return await _context.Objectives
                .OrderBy(o => o.SortOrder)
                .Select(o => new ObjectiveSimpleDto
                {
                    Id = o.Id,
                    Title = o.Title,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();
        }

        public async Task CreateObjectiveAsync(CreateObjectiveDto request, Guid userId, Guid establishmentId)
        {
            DateTime? eventDateUtc = request.EventDate?.ToUniversalTime();
            DateTime? endDateUtc = request.EndDate?.ToUniversalTime();
            DateTime? displayStartUtc = request.DisplayStartDate?.ToUniversalTime();
            DateTime? displayEndUtc = request.DisplayEndDate?.ToUniversalTime();

            var prerequisiteObjectives = new List<Objective>();
            if (request.PrerequisiteObjectiveIds != null && request.PrerequisiteObjectiveIds.Any())
            {
                prerequisiteObjectives = await _context.Objectives
                    .Where(o => request.PrerequisiteObjectiveIds.Contains(o.Id))
                    .ToListAsync();
            }

            var objective = new Objective
            {
                Id = Guid.NewGuid(),
                EstablishmentId = establishmentId,
                Title = request.Title,
                Description = request.Description,
                XpReward = request.XpReward,
                DocPointsReward = request.DocPointsReward,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = request.IsActive,
                IsUnique = request.IsUnique,
                FrequencyHours = request.FrequencyHours,
                EventDate = eventDateUtc,
                EndDate = endDateUtc,
                DisplayStartDate = displayStartUtc,
                DisplayEndDate = displayEndUtc,
                Location = request.Location ?? string.Empty,
                IconName = request.IconName ?? "Star",
                Color = request.Color,
                Prerequisites = prerequisiteObjectives,
                LifespanHours = request.LifespanHours,
                Category = DetermineCategory(request),
                SortOrder = request.SortOrder,
                IsStreakEnabled = request.IsStreakEnabled,
                StreakTerminalHours = request.StreakTerminalHours,
                StreakFrequency = request.StreakFrequency,
                StreakExcludedDays = request.StreakExcludedDays,
                StreakExcludedMonths = request.StreakExcludedMonths
            };

            _context.Objectives.Add(objective);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateObjectiveAsync(Guid id, CreateObjectiveDto request)
        {
            var objective = await _context.Objectives
                .Include(o => o.Prerequisites)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (objective == null) return false;

            // Update prerequisites
            objective.Prerequisites.Clear();
            if (request.PrerequisiteObjectiveIds != null && request.PrerequisiteObjectiveIds.Any())
            {
                var newPrerequisites = await _context.Objectives
                    .Where(o => request.PrerequisiteObjectiveIds.Contains(o.Id))
                    .ToListAsync();
                objective.Prerequisites = newPrerequisites;
            }

            // Update fields
            objective.Title = request.Title;
            objective.Description = request.Description;
            objective.XpReward = request.XpReward;
            objective.DocPointsReward = request.DocPointsReward;
            objective.IsActive = request.IsActive;
            objective.IsUnique = request.IsUnique;
            objective.FrequencyHours = request.FrequencyHours;
            objective.EventDate = request.EventDate?.ToUniversalTime();
            objective.EndDate = request.EndDate?.ToUniversalTime();
            objective.DisplayStartDate = request.DisplayStartDate?.ToUniversalTime();
            objective.DisplayEndDate = request.DisplayEndDate?.ToUniversalTime();
            objective.Location = request.Location ?? string.Empty;
            objective.IconName = request.IconName ?? "Star";
            objective.Color = request.Color;
            objective.LifespanHours = request.LifespanHours;
            
            // Automatic Categorization Logic
            objective.Category = DetermineCategory(request);
            objective.SortOrder = request.SortOrder;
            objective.IsStreakEnabled = request.IsStreakEnabled;
            objective.StreakTerminalHours = request.StreakTerminalHours;
            objective.StreakFrequency = request.StreakFrequency;
            objective.StreakExcludedDays = request.StreakExcludedDays;
            objective.StreakExcludedMonths = request.StreakExcludedMonths;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteObjectiveAsync(Guid id)
        {
            var objective = await _context.Objectives.FindAsync(id);
            if (objective == null) return false;

            _context.Objectives.Remove(objective);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ObjectiveCategory DetermineCategory(CreateObjectiveDto request)
        {
            // 1. Onboarding handled manually now via dropdown
            
            // 2. Event -> Evenement (Unique + Start + End)
            // The prompt says "Ce sont les objectifs uniques avec une date de début et de fin"
            // We'll check if EventDate and EndDate are set. IsUnique is usually true for events but let's stick to dates.
            if (request.EventDate.HasValue && request.EndDate.HasValue) return ObjectiveCategory.Evenement;

            // 3. Reload -> Rechargement (Frequency) - REMOVED per new requirements
            // if (request.FrequencyHours.HasValue) return ObjectiveCategory.Rechargement;

            // 4. Default / Manual
            return request.Category;
        }

        public async Task<bool> ReorderObjectivesAsync(List<Guid> orderedIds)
        {
            if (orderedIds == null || !orderedIds.Any()) return false;

            var objectives = await _context.Objectives
                .Where(o => orderedIds.Contains(o.Id))
                .ToListAsync();

            if (!objectives.Any()) return false;

            foreach (var obj in objectives)
            {
                var index = orderedIds.IndexOf(obj.Id);
                if (index != -1)
                {
                    obj.SortOrder = index;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private static ObjectiveDto MapToDto(Objective obj, bool isAlreadyCompleted)
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
                IsAlreadyCompleted = isAlreadyCompleted,
                PrerequisiteObjectiveIds = obj.Prerequisites?.Select(p => p.Id).ToList() ?? new List<Guid>(),
                LifespanHours = obj.LifespanHours,
                Category = obj.Category,
                UnlockedObjectiveTitles = obj.IsPrerequisiteFor?.Select(x => x.Title).ToList() ?? new List<string>(),
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
