using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamifyMe.Shared.Constants;
using GamifyMe.Api.Services;

namespace GamifyMe.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
    public class SettingsController : ControllerBase
    {
        private readonly CurrencyService _currencyService;

        public SettingsController(CurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpGet("currency")]
        public ActionResult<string> GetCurrency()
        {
            return Ok(_currencyService.CurrencyName);
        }

        [HttpPost("currency")]
        public IActionResult SetCurrency([FromBody] string newCurrency)
        {
            if (string.IsNullOrWhiteSpace(newCurrency))
                return BadRequest("Currency name cannot be empty.");
            _currencyService.CurrencyName = newCurrency.Trim();
            return NoContent();
        }
    }
}
