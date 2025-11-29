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

            // 1. Get completed objectives for this user
            var completedObjectiveIds = await _context.Validations
                .Where(v => v.UserId == userId)
                .Select(v => v.ObjectiveId)
                .ToListAsync();
            var completedSet = completedObjectiveIds.ToHashSet();

            // 2. Get active objectives for the establishment
            var allActiveObjectives = await _context.Objectives
                .Include(o => o.Prerequisites)
                .Where(o => o.EstablishmentId == establishmentId
                            && o.IsActive
                            && (o.EndDate == null || o.EndDate > now)
                            && (o.DisplayStartDate == null || o.DisplayStartDate <= now)
                            && (o.DisplayEndDate == null || o.DisplayEndDate >= now))
                .AsNoTracking()
                .ToListAsync();

            var resultList = new List<ObjectiveDto>();

            foreach (var obj in allActiveObjectives)
            {
                // Check prerequisites
                bool isLocked = false;
                if (obj.Prerequisites != null && obj.Prerequisites.Any())
                {
                    if (!obj.Prerequisites.All(p => completedSet.Contains(p.Id)))
                        isLocked = true;
                }
                if (isLocked) continue;

                // Check if already completed (for unique objectives)
                bool alreadyDone = obj.IsUnique && completedSet.Contains(obj.Id);
                if (alreadyDone) continue;

                resultList.Add(MapToDto(obj, alreadyDone));
            }

            return resultList.OrderBy(o => o.EventDate ?? DateTime.MaxValue).ToList();
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
                Prerequisites = prerequisiteObjectives
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
                PrerequisiteObjectiveIds = obj.Prerequisites?.Select(p => p.Id).ToList() ?? new List<Guid>()
            };
        }
    }
}
