using GamifyMe.Api.Constants;
using GamifyMe.Api.Services;
using GamifyMe.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GamifyMe.Api.Controllers
{
    [Route("api/user-badges")]
    [ApiController]
    [Authorize]
    public class UserBadgesController : ControllerBase
    {
        private readonly BadgesService _badgesService;

        public UserBadgesController(BadgesService badgesService)
        {
            _badgesService = badgesService;
        }

        [HttpGet("")]
        public async Task<ActionResult<List<BadgeDto>>> GetMyBadges()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var badges = await _badgesService.GetAllBadgesAsync(userId);
            return Ok(badges);
        }

        [HttpPost("check")]
        public async Task<ActionResult<List<BadgeDto>>> CheckUnlocks()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var establishmentIdClaim = User.FindFirstValue("EstablishmentId");
            if (string.IsNullOrEmpty(establishmentIdClaim)) return BadRequest("Establishment ID not found.");

            var newBadges = await _badgesService.CheckAndUnlockBadgesAsync(userId, Guid.Parse(establishmentIdClaim));
            return Ok(newBadges);
        }

        [HttpPost("{badgeId}/favorite")]
        public async Task<IActionResult> SetFavorite(Guid badgeId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _badgesService.SetFavoriteBadgeAsync(userId, badgeId);
            
            if (!success) return BadRequest("Badge non trouvé ou non débloqué.");
            
            return Ok();
        }
    }
}
