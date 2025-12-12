using GamifyMe.Api.Constants;
using GamifyMe.Api.Data;
using GamifyMe.Api.Services;
using GamifyMe.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GamifyMe.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ObjectivesController : ControllerBase
    {
        private readonly ObjectiveService _objectiveService;
        private readonly DataContext _context; // Still needed for user lookup if not moved to service

        public ObjectivesController(ObjectiveService objectiveService, DataContext context)
        {
            _objectiveService = objectiveService;
            _context = context;
        }

        // GET api/objectives/active
        [HttpGet("active")]
        public async Task<ActionResult<List<ObjectiveDto>>> GetActiveObjectives()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Utilisateur non identifié.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized("Utilisateur introuvable.");

            var objectives = await _objectiveService.GetActiveObjectivesAsync(userId, user.EstablishmentId);
            return Ok(objectives);
        }

        // POST api/objectives
        [HttpPost]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> CreateObjective(CreateObjectiveDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            await _objectiveService.CreateObjectiveAsync(request, userId, establishmentId);
            return Ok();
        }

        // GET api/objectives/list-all
        [HttpGet("list-all")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<ActionResult<List<ObjectiveSimpleDto>>> GetAllObjectivesSimpleList()
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var objectives = await _objectiveService.GetAllObjectivesSimpleListAsync(establishmentId);
            return Ok(objectives);
        }

        // GET api/objectives/list-all-full
        [HttpGet("list-all-full")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<ActionResult<List<ObjectiveDto>>> GetAllObjectivesFullList()
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var objectives = await _objectiveService.GetAllObjectivesFullListAsync(establishmentId);
            return Ok(objectives);
        }

        // PUT api/objectives/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> UpdateObjective(Guid id, CreateObjectiveDto request)
        {
            var success = await _objectiveService.UpdateObjectiveAsync(id, request);
            if (!success) return NotFound("Objectif non trouvé.");
            return NoContent();
        }

        // DELETE api/objectives/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> DeleteObjective(Guid id)
        {
            var success = await _objectiveService.DeleteObjectiveAsync(id);
            if (!success) return NotFound("Objectif non trouvé.");
            return NoContent();
        }
        [HttpPost("reorder")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> ReorderObjectives(ReorderRequestDto request)
        {
            var success = await _objectiveService.ReorderObjectivesAsync(request.OrderedIds);
            if (!success) return BadRequest("Erreur lors de la réorganisation.");
            return Ok();
        }
    }
}