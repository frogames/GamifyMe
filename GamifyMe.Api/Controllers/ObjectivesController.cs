using GamifyMe.Api.Constants;
using GamifyMe.Api.Data;
using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <-- CORRECTION: USING MANQUANT
using System.Security.Claims;

namespace GamifyMe.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ObjectivesController : ControllerBase
    {
        private readonly DataContext _context;

        public ObjectivesController(DataContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> CreateObjective(CreateObjectiveDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            DateTime? eventDateUtc = request.EventDate.HasValue ? request.EventDate.Value.ToUniversalTime() : null;
            DateTime? endDateUtc = request.EndDate.HasValue ? request.EndDate.Value.ToUniversalTime() : null;

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
                EstablishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!),
                Title = request.Title,
                Description = request.Description,
                XpReward = request.XpReward,
                DocPointsReward = request.DocPointsReward,
                CreatedById = Guid.Parse(userId!),
                CreatedAt = DateTime.UtcNow,
                IsActive = request.IsActive,
                IsUnique = request.IsUnique,
                EventDate = eventDateUtc,
                EndDate = endDateUtc,
                Location = request.Location ?? string.Empty,
                IconName = request.IconName ?? "Star",
                Prerequisites = prerequisiteObjectives
            };

            _context.Objectives.Add(objective);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<ObjectiveDto>>> GetActiveObjectives()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var now = DateTime.UtcNow;

            var allCompletedObjectiveIds = await _context.Validations
                .Where(v => v.UserId == userId)
                .Select(v => v.ObjectiveId)
                .ToHashSetAsync(); // <-- Cette ligne va maintenant compiler

            var allActiveObjectives = await _context.Objectives
                .Include(o => o.Prerequisites)
                .Where(o => o.IsActive && (o.EndDate == null || o.EndDate.Value > now))
                .ToListAsync();

            var unlockedObjectives = allActiveObjectives.Where(objective =>
            {
                if (!objective.Prerequisites.Any()) return true;
                return objective.Prerequisites.All(prereq => allCompletedObjectiveIds.Contains(prereq.Id));
            });

            var resultDtoList = unlockedObjectives
                .Select(o => new ObjectiveDto
                {
                    Id = o.Id,
                    Title = o.Title,
                    Description = o.Description,
                    IconName = o.IconName,
                    XpReward = o.XpReward,
                    DocPointsReward = o.DocPointsReward,
                    Location = o.Location,
                    EventDate = o.EventDate,
                    EndDate = o.EndDate,
                    IsUnique = o.IsUnique,
                    IsAlreadyCompleted = o.IsUnique && allCompletedObjectiveIds.Contains(o.Id)
                })
                .Where(dto => !dto.IsAlreadyCompleted)
                .OrderByDescending(o => o.EventDate)
                .ToList();

            return Ok(resultDtoList);
        }

        [HttpGet("list-all")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<ActionResult<List<ObjectiveSimpleDto>>> GetAllObjectivesSimpleList()
        {
            var objectives = await _context.Objectives
                .OrderBy(o => o.Title)
                .Select(o => new ObjectiveSimpleDto
                {
                    Id = o.Id,
                    Title = o.Title
                })
                .ToListAsync();
            return Ok(objectives);
        }

        [HttpGet("list-all-full")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<ActionResult<List<ObjectiveDto>>> GetAllObjectivesFullList()
        {
            var objectives = await _context.Objectives
                .Include(o => o.Prerequisites)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new ObjectiveDto
                {
                    Id = o.Id,
                    Title = o.Title,
                    Description = o.Description,
                    IconName = o.IconName,
                    XpReward = o.XpReward,
                    DocPointsReward = o.DocPointsReward,
                    Location = o.Location,
                    EventDate = o.EventDate,
                    EndDate = o.EndDate,
                    IsUnique = o.IsUnique,
                    IsActive = o.IsActive,
                    IsAlreadyCompleted = false,
                    PrerequisiteObjectiveIds = o.Prerequisites.Select(p => p.Id).ToList()
                })
                .ToListAsync();
            return Ok(objectives);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> UpdateObjective(Guid id, CreateObjectiveDto request)
        {
            var objective = await _context.Objectives
                .Include(o => o.Prerequisites)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (objective == null) return NotFound("Objectif non trouvé.");

            DateTime? eventDateUtc = request.EventDate.HasValue ? request.EventDate.Value.ToUniversalTime() : null;
            DateTime? endDateUtc = request.EndDate.HasValue ? request.EndDate.Value.ToUniversalTime() : null;

            objective.Prerequisites.Clear();
            if (request.PrerequisiteObjectiveIds != null && request.PrerequisiteObjectiveIds.Any())
            {
                var newPrerequisites = await _context.Objectives
                    .Where(o => request.PrerequisiteObjectiveIds.Contains(o.Id))
                    .ToListAsync();
                objective.Prerequisites = newPrerequisites;
            }

            objective.Title = request.Title;
            objective.Description = request.Description;
            objective.XpReward = request.XpReward;
            objective.DocPointsReward = request.DocPointsReward;
            objective.IsActive = request.IsActive;
            objective.IsUnique = request.IsUnique;
            objective.EventDate = eventDateUtc;
            objective.EndDate = endDateUtc;
            objective.Location = request.Location ?? string.Empty;
            objective.IconName = request.IconName ?? "Star";

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> DeleteObjective(Guid id)
        {
            var objective = await _context.Objectives.FindAsync(id);
            if (objective == null) return NotFound("Objectif non trouvé.");

            _context.Objectives.Remove(objective);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}