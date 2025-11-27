using GamifyMe.Api.Constants;
using GamifyMe.Api.Data;
using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET api/objectives/active
        [HttpGet("active")]
        public async Task<ActionResult<List<ObjectiveDto>>> GetActiveObjectives()
        {
            // 1. Récupérer l'ID utilisateur de manière sécurisée
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Utilisateur non identifié.");
            }

            // 2. Récupérer l'utilisateur pour avoir son EstablishmentId
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized("Utilisateur introuvable.");

            var now = DateTime.UtcNow;

            // 3. Récupérer les IDs des objectifs déjà validés par cet utilisateur
            var completedObjectiveIds = await _context.Validations
                .Where(v => v.UserId == userId)
                .Select(v => v.ObjectiveId)
                .ToListAsync();
            var completedSet = completedObjectiveIds.ToHashSet();

            // 4. Récupérer tous les objectifs ACTIFS de l'établissement avec filtres d'affichage
            var allActiveObjectives = await _context.Objectives
                .Include(o => o.Prerequisites)
                .Where(o => o.EstablishmentId == user.EstablishmentId
                            && o.IsActive
                            && (o.EndDate == null || o.EndDate > now)
                            && (o.DisplayStartDate == null || o.DisplayStartDate <= now)
                            && (o.DisplayEndDate == null || o.DisplayEndDate >= now))
                .AsNoTracking()
                .ToListAsync();

            var resultList = new List<ObjectiveDto>();

            foreach (var obj in allActiveObjectives)
            {
                // Vérification des prérequis
                bool isLocked = false;
                if (obj.Prerequisites != null && obj.Prerequisites.Any())
                {
                    if (!obj.Prerequisites.All(p => completedSet.Contains(p.Id)))
                        isLocked = true;
                }
                if (isLocked) continue;

                // Vérification si déjà complété (pour les objectifs uniques)
                bool alreadyDone = obj.IsUnique && completedSet.Contains(obj.Id);
                if (alreadyDone) continue;

                resultList.Add(new ObjectiveDto
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
                    IsAlreadyCompleted = alreadyDone
                });
            }

            // Tri final : Les événements les plus proches d'abord
            return Ok(resultList.OrderBy(o => o.EventDate ?? DateTime.MaxValue).ToList());
        }

        // POST api/objectives
        [HttpPost]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> CreateObjective(CreateObjectiveDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            DateTime? eventDateUtc = request.EventDate.HasValue ? request.EventDate.Value.ToUniversalTime() : null;
            DateTime? endDateUtc = request.EndDate.HasValue ? request.EndDate.Value.ToUniversalTime() : null;
            DateTime? displayStartUtc = request.DisplayStartDate.HasValue ? request.DisplayStartDate.Value.ToUniversalTime() : null;
            DateTime? displayEndUtc = request.DisplayEndDate.HasValue ? request.DisplayEndDate.Value.ToUniversalTime() : null;

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
            return Ok();
        }

        // GET api/objectives/list-all
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

        // GET api/objectives/list-all-full
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
                    DisplayStartDate = o.DisplayStartDate,
                    DisplayEndDate = o.DisplayEndDate,
                    IsUnique = o.IsUnique,
                    FrequencyHours = o.FrequencyHours,
                    IsActive = o.IsActive,
                    IsAlreadyCompleted = false,
                    PrerequisiteObjectiveIds = o.Prerequisites.Select(p => p.Id).ToList()
                })
                .ToListAsync();
            return Ok(objectives);
        }

        // PUT api/objectives/{id}
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
            DateTime? displayStartUtc = request.DisplayStartDate.HasValue ? request.DisplayStartDate.Value.ToUniversalTime() : null;
            DateTime? displayEndUtc = request.DisplayEndDate.HasValue ? request.DisplayEndDate.Value.ToUniversalTime() : null;

            // Update prerequisites
            objective.Prerequisites.Clear();
            if (request.PrerequisiteObjectiveIds != null && request.PrerequisiteObjectiveIds.Any())
            {
                var newPrerequisites = await _context.Objectives
                    .Where(o => request.PrerequisiteObjectiveIds.Contains(o.Id))
                    .ToListAsync();
                objective.Prerequisites = newPrerequisites;
            }

            // Update other fields
            objective.Title = request.Title;
            objective.Description = request.Description;
            objective.XpReward = request.XpReward;
            objective.DocPointsReward = request.DocPointsReward;
            objective.IsActive = request.IsActive;
            objective.IsUnique = request.IsUnique;
            objective.FrequencyHours = request.FrequencyHours;
            objective.EventDate = eventDateUtc;
            objective.EndDate = endDateUtc;
            objective.DisplayStartDate = displayStartUtc;
            objective.DisplayEndDate = displayEndUtc;
            objective.Location = request.Location ?? string.Empty;
            objective.IconName = request.IconName ?? "Star";

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/objectives/{id}
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