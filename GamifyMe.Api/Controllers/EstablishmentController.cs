using GamifyMe.Api.Data;
using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using GamifyMe.Shared.Constants;

namespace GamifyMe.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")] // Only admins can access establishment settings
    public class EstablishmentController : ControllerBase
    {
        private readonly DataContext _context;

        public EstablishmentController(DataContext context)
        {
            _context = context;
        }

        private Guid GetCurrentEstablishmentId()
        {
            var claim = User.FindFirst("EstablishmentId");
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
        }

        [HttpGet("settings")]
        public async Task<ActionResult<EstablishmentSettingsDto>> GetSettings()
        {
            var establishmentId = GetCurrentEstablishmentId();
            if (establishmentId == Guid.Empty) return Unauthorized();

            var establishment = await _context.Establishments.FindAsync(establishmentId);
            if (establishment == null) return NotFound("Établissement introuvable.");

            return Ok(new EstablishmentSettingsDto
            {
                Id = establishment.Id,
                Name = establishment.Name,
                CurrencyName = establishment.CurrencyName,
                ArchiveUsersAfterInactiveDays = establishment.ArchiveUsersAfterInactiveDays,
                MaxUsers = establishment.MaxUsers,
                IsShopEnabled = establishment.IsShopEnabled,
                IsGroupsEnabled = establishment.IsGroupsEnabled,
                IsChallengesEnabled = establishment.IsChallengesEnabled
            });
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateSettings(EstablishmentSettingsDto request)
        {
            var establishmentId = GetCurrentEstablishmentId();
            if (establishmentId == Guid.Empty) return Unauthorized();

            var establishment = await _context.Establishments.FindAsync(establishmentId);
            if (establishment == null) return NotFound("Établissement introuvable.");

            if (establishment.Id != request.Id) return BadRequest("ID de l'établissement incorrect.");

            establishment.Name = request.Name;
            establishment.CurrencyName = request.CurrencyName;
            establishment.ArchiveUsersAfterInactiveDays = request.ArchiveUsersAfterInactiveDays;
            establishment.IsShopEnabled = request.IsShopEnabled;
            establishment.IsGroupsEnabled = request.IsGroupsEnabled;
            establishment.IsChallengesEnabled = request.IsChallengesEnabled;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("reset-credits")]
        public async Task<IActionResult> ResetCredits()
        {
            var establishmentId = GetCurrentEstablishmentId();
            if (establishmentId == Guid.Empty) return Unauthorized();

            // Find the currency code. For now, it's safer to find non-XP wallets. 
            // Or assume "DOC" or the newly set CurrencyName? 
            // The wallet stores "CurrencyCode". 
            // Usually we have "XP" and another one (e.g. "DOC").
            // If the user changed the CurrencyName, does it change the Wallet CurrencyCode?
            // The existing Register logic sets "DOC". 
            // The current implementation of Wallets seems to rely on "XP" vs not "XP".
            
            // To be safe and respect the rule "fixer à 0 les crédits de tous les users", 
            // we should target the non-XP wallets for this establishment.
            
            var walletsToReset = await _context.Wallets
                .Where(w => w.EstablishmentId == establishmentId && w.CurrencyCode != "XP")
                .ToListAsync();

            foreach (var wallet in walletsToReset)
            {
                wallet.Balance = 0;
            }

            var groupsToReset = await _context.Groups
                .Where(g => g.EstablishmentId == establishmentId)
                .ToListAsync();

            foreach (var group in groupsToReset)
            {
                group.TotalXp = 0;
            }

            await _context.SaveChangesAsync();

            return Ok($"Réinitialisation effectuée : {walletsToReset.Count} portefeuilles de crédits et {groupsToReset.Count} groupes remis à zéro.");
        }

        [HttpGet("stats")]
        public async Task<ActionResult<EstablishmentStatsDto>> GetStats()
        {
            var establishmentId = GetCurrentEstablishmentId();
            if (establishmentId == Guid.Empty) return Unauthorized();

            var userCount = await _context.Users.CountAsync(u => u.EstablishmentId == establishmentId);
            var establishment = await _context.Establishments.FindAsync(establishmentId);

            return Ok(new EstablishmentStatsDto
            {
                UserCount = userCount,
                MaxUsers = establishment?.MaxUsers ?? 0,
                SystemHealth = "Excellent"
            });
        }
    }
}
