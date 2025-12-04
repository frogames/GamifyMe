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
    [Route("api/groups")]
    [ApiController]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly DataContext _context;

        public GroupsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<GroupDto>>> GetGroups()
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var groups = await _context.Groups
                .Where(g => g.EstablishmentId == establishmentId)
                .Include(g => g.Members)
                .Select(g => new GroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    IconName = g.IconName,
                    Color = g.Color,
                    TotalXp = g.TotalXp,
                    MemberCount = g.Members.Count,
                    RegistrationDurationHours = g.RegistrationDurationHours,
                    CreatedAt = g.CreatedAt
                })
                .ToListAsync();

            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupDto>> GetGroup(Guid id)
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id && g.EstablishmentId == establishmentId);

            if (group == null) return NotFound("Groupe introuvable.");

            return Ok(new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                IconName = group.IconName,
                Color = group.Color,
                TotalXp = group.TotalXp,
                MemberCount = group.Members.Count,
                RegistrationDurationHours = group.RegistrationDurationHours,
                CreatedAt = group.CreatedAt
            });
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<ActionResult<GroupDto>> CreateGroup(CreateGroupDto request)
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);

            var group = new Group
            {
                Id = Guid.NewGuid(),
                EstablishmentId = establishmentId,
                Name = request.Name,
                Description = request.Description,
                IconName = request.IconName,
                Color = request.Color,
                TotalXp = 0,
                RegistrationDurationHours = request.RegistrationDurationHours,
                CreatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            return Ok(new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                IconName = group.IconName,
                Color = group.Color,
                TotalXp = group.TotalXp,
                MemberCount = 0,
                RegistrationDurationHours = group.RegistrationDurationHours,
                CreatedAt = group.CreatedAt
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<ActionResult<GroupDto>> UpdateGroup(Guid id, UpdateGroupDto request)
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id && g.EstablishmentId == establishmentId);

            if (group == null) return NotFound("Groupe introuvable.");

            group.Name = request.Name;
            group.Description = request.Description;
            group.IconName = request.IconName;
            group.Color = request.Color;
            group.RegistrationDurationHours = request.RegistrationDurationHours;

            await _context.SaveChangesAsync();

            return Ok(new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                IconName = group.IconName,
                Color = group.Color,
                TotalXp = group.TotalXp,
                MemberCount = await _context.Users.CountAsync(u => u.GroupId == group.Id),
                RegistrationDurationHours = group.RegistrationDurationHours,
                CreatedAt = group.CreatedAt
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<ActionResult> DeleteGroup(Guid id)
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id && g.EstablishmentId == establishmentId);

            if (group == null) return NotFound("Groupe introuvable.");

            // Optionnel : Gérer les membres (les retirer du groupe ?)
            var members = await _context.Users.Where(u => u.GroupId == id).ToListAsync();
            foreach (var member in members)
            {
                member.GroupId = null;
            }

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return Ok("Groupe supprimé.");
        }

        [HttpPost("join/{groupId}")]
        public async Task<ActionResult> JoinGroup(Guid groupId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);

            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId && g.EstablishmentId == establishmentId);
            if (group == null) return NotFound("Groupe introuvable.");

            // Check Registration Duration
            if (group.RegistrationDurationHours.HasValue && group.RegistrationDurationHours.Value > 0)
            {
                var expirationDate = group.CreatedAt.AddHours(group.RegistrationDurationHours.Value);
                if (DateTime.UtcNow > expirationDate)
                {
                    return BadRequest("Les inscriptions pour ce groupe sont closes.");
                }
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Utilisateur introuvable.");

            user.GroupId = groupId;
            await _context.SaveChangesAsync();

            return Ok($"Vous avez rejoint le groupe {group.Name}.");
        }

        [HttpPost("leave")]
        public async Task<ActionResult> LeaveGroup()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Utilisateur introuvable.");

            user.GroupId = null;
            await _context.SaveChangesAsync();

            return Ok("Vous avez quitté votre groupe.");
        }
    }
}
