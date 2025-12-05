using System;
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
    [Route("api/bonus-periods")]
    [ApiController]
    [Authorize]
    public class BonusPeriodsController : ControllerBase
    {
        private readonly DataContext _context;

        public BonusPeriodsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("current")]
        public async Task<ActionResult<BonusPeriodDto?>> GetCurrentBonus()
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var now = DateTime.UtcNow;

            var activeBonus = await _context.BonusPeriods
                .Where(b => b.EstablishmentId == establishmentId && b.IsActive && b.StartDate <= now && b.EndDate >= now)
                .OrderByDescending(b => b.StartDate)
                .FirstOrDefaultAsync();

            if (activeBonus == null) return Ok(null);

            return Ok(new BonusPeriodDto
            {
                Id = activeBonus.Id,
                Name = activeBonus.Name,
                StartDate = activeBonus.StartDate,
                EndDate = activeBonus.EndDate,
                Type = activeBonus.Type,
                Multiplier = activeBonus.Multiplier,
                IsActive = activeBonus.IsActive
            });
        }

        [HttpGet]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<ActionResult<List<BonusPeriodDto>>> GetAll()
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var list = await _context.BonusPeriods
                .Where(b => b.EstablishmentId == establishmentId)
                .OrderByDescending(b => b.StartDate)
                .Select(b => new BonusPeriodDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    Type = b.Type,
                    Multiplier = b.Multiplier,
                    IsActive = b.IsActive
                })
                .ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<ActionResult> Create(CreateBonusPeriodDto request)
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);

            if (request.StartDate >= request.EndDate)
                return BadRequest("La date de début doit être avant la date de fin.");

            if (request.IsActive)
            {
                // Ensure we compare UTC with UTC
                var startUtc = request.StartDate.ToUniversalTime();
                var endUtc = request.EndDate.ToUniversalTime();

                bool overlap = await _context.BonusPeriods.AnyAsync(b => 
                    b.EstablishmentId == establishmentId && 
                    b.IsActive &&
                    ((startUtc >= b.StartDate && startUtc < b.EndDate) ||
                     (endUtc > b.StartDate && endUtc <= b.EndDate) ||
                     (startUtc <= b.StartDate && endUtc >= b.EndDate)));
                
                if (overlap) return BadRequest("Une autre période bonus est déjà active sur ce créneau.");
            }

            var bonus = new BonusPeriod
            {
                Id = Guid.NewGuid(),
                EstablishmentId = establishmentId,
                Name = request.Name,
                StartDate = request.StartDate.ToUniversalTime(),
                EndDate = request.EndDate.ToUniversalTime(),
                Type = request.Type,
                Multiplier = request.Multiplier,
                IsActive = request.IsActive
            };

            _context.BonusPeriods.Add(bonus);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<ActionResult> Update(Guid id, UpdateBonusPeriodDto request)
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var bonus = await _context.BonusPeriods.FirstOrDefaultAsync(b => b.Id == id && b.EstablishmentId == establishmentId);
            
            if (bonus == null) return NotFound();

            if (request.StartDate >= request.EndDate)
                return BadRequest("La date de début doit être avant la date de fin.");

            if (request.IsActive)
            {
                // Ensure we compare UTC with UTC
                var startUtc = request.StartDate.ToUniversalTime();
                var endUtc = request.EndDate.ToUniversalTime();

                bool overlap = await _context.BonusPeriods.AnyAsync(b => 
                    b.Id != id &&
                    b.EstablishmentId == establishmentId && 
                    b.IsActive &&
                    ((startUtc >= b.StartDate && startUtc < b.EndDate) ||
                     (endUtc > b.StartDate && endUtc <= b.EndDate) ||
                     (startUtc <= b.StartDate && endUtc >= b.EndDate)));
                
                if (overlap) return BadRequest("Une autre période bonus est déjà active sur ce créneau.");
            }

            bonus.Name = request.Name;
            bonus.StartDate = request.StartDate.ToUniversalTime();
            bonus.EndDate = request.EndDate.ToUniversalTime();
            bonus.Type = request.Type;
            bonus.Multiplier = request.Multiplier;
            bonus.IsActive = request.IsActive;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var bonus = await _context.BonusPeriods.FirstOrDefaultAsync(b => b.Id == id && b.EstablishmentId == establishmentId);
            
            if (bonus == null) return NotFound();

            _context.BonusPeriods.Remove(bonus);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
