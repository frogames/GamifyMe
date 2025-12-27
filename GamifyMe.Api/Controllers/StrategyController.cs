using GamifyMe.Api.Services;
using GamifyMe.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GamifyMe.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Admin,Coach")]
    public class StrategyController : ControllerBase
    {
        private readonly StrategyConfigurationService _strategyService;

        public StrategyController(StrategyConfigurationService strategyService)
        {
            _strategyService = strategyService;
        }

        private Guid GetCurrentEstablishmentId()
        {
            var claim = User.FindFirst("EstablishmentId");
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
        }

        [HttpGet]
        public async Task<ActionResult<GamificationStrategyDto>> GetStrategy()
        {
            var establishmentId = GetCurrentEstablishmentId();
            if (establishmentId == Guid.Empty) return Unauthorized();

            var strategy = await _strategyService.GetStrategyAsync(establishmentId);
            return Ok(strategy);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateStrategy(GamificationStrategyDto dto)
        {
            var establishmentId = GetCurrentEstablishmentId();
            if (establishmentId == Guid.Empty) return Unauthorized();

            await _strategyService.UpdateStrategyAsync(establishmentId, dto);
            return NoContent();
        }
    }
}
