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

            // Build Chain Info (x/y)
            // We assume a linear chain for simplicity as requested ("single prerequisite")
            // But we must handle the graph structure safely.
            var objectiveDtos = new List<ObjectiveDto>();
            
            // Helper dictionary for quick lookup
            var objMap = validObjectives.ToDictionary(o => o.Id);

            foreach (var obj in validObjectives)
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
                    // Note: Parent might not be in 'validObjectives' if it's expired/inactive, 
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

                var dto = MapToDto(obj, isAlreadyCompleted);
                dto.NextAvailableDate = nextAvailableDate;

                if (obj.LifespanHours.HasValue)
                {
                    var startTime = obj.DisplayStartDate ?? obj.CreatedAt;
                    dto.ExpirationDate = startTime.AddHours(obj.LifespanHours.Value);
                }
                
                if (chainLen > 1)
                {
                    dto.Title = $"{dto.Title} ({chainPos}/{chainLen})";
                    dto.ChainPosition = chainPos;
                    dto.ChainLength = chainLen;
                }

                objectiveDtos.Add(dto);
            }

            return objectiveDtos.OrderBy(o => o.EventDate ?? DateTime.MaxValue).ToList();
        }

        private int GetMaxChainDepth(Objective current, List<Objective> allObjectives, HashSet<Guid> visited)
        {
            if (visited.Contains(current.Id)) return 0; // Cycle detected
            visited.Add(current.Id);

            // Find children: objectives that have 'current' as a prerequisite
            // We can use IsPrerequisiteFor if loaded, or search the list
            var children = allObjectives.Where(o => o.Prerequisites.Any(p => p.Id == current.Id)).ToList();
            
            if (!children.Any()) return 1;

            int maxChildDepth = 0;
            foreach (var child in children)
            {
                int depth = GetMaxChainDepth(child, allObjectives, new HashSet<Guid>(visited));
                if (depth > maxChildDepth) maxChildDepth = depth;
            }

            return 1 + maxChildDepth;
        }

        public async Task<List<ObjectiveDto>> GetAllObjectivesFullListAsync()
        {
            var objectives = await _context.Objectives
                .Include(o => o.Prerequisites)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return objectives.Select(o => MapToDto(o, false)).ToList();
        }

        public async Task<List<ObjectiveSimpleDto>> GetAllObjectivesSimpleListAsync()
        {
            return await _context.Objectives
                .OrderBy(o => o.Title)
                .Select(o => new ObjectiveSimpleDto
                {
                    Id = o.Id,
                    Title = o.Title
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
                Prerequisites = prerequisiteObjectives,
                LifespanHours = request.LifespanHours
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
            objective.LifespanHours = request.LifespanHours;

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

        private static ObjectiveDto MapToDto(Objective obj, bool isAlreadyCompleted)
        {
            return new ObjectiveDto
            {
                Id = obj.Id,
                Title = obj.Title,
                Description = obj.Description,
                IconName = obj.IconName,
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
                LifespanHours = obj.LifespanHours
            };
        }
    }
}
