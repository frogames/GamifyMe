using GamifyMe.Api.Constants;
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
    public class StoreController : ControllerBase
    {
        private readonly StoreService _storeService;

        public StoreController(StoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<StoreItemDto>>> GetActiveStoreItems()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var items = await _storeService.GetActiveStoreItemsAsync(userId);
            return Ok(items);
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> CreateStoreItem(StoreItemDto request)
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var resultDto = await _storeService.CreateStoreItemAsync(request, establishmentId);
            return CreatedAtAction(nameof(GetStoreItemById), new { id = resultDto.Id }, resultDto);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur},{Roles.Gestionnaire}")]
        public async Task<ActionResult<StoreItemDto>> GetStoreItemById(Guid id)
        {
            var dto = await _storeService.GetStoreItemByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpGet("list-all")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur},{Roles.Gestionnaire}")]
        public async Task<ActionResult<List<StoreItemDto>>> GetAllStoreItemsSimpleList()
        {
            var items = await _storeService.GetAllStoreItemsSimpleListAsync();
            return Ok(items);
        }

        [HttpPost("purchase/{itemId}")]
        [Authorize]
        public async Task<IActionResult> PurchaseStoreItem(Guid itemId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var establishmentIdClaim = User.FindFirstValue("EstablishmentId");
            Guid? establishmentId = establishmentIdClaim != null ? Guid.Parse(establishmentIdClaim) : null;

            var (success, message) = await _storeService.PurchaseStoreItemAsync(itemId, userId, establishmentId);
            
            if (!success)
            {
                // Determine status code based on message or just return BadRequest/500
                // For simplicity, using BadRequest for logic errors and 500 for exceptions caught in service
                if (message.StartsWith("Erreur d'achat")) return StatusCode(500, message);
                return BadRequest(message);
            }

            return Ok(message);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> UpdateStoreItem(Guid id, StoreItemDto request)
        {
            var success = await _storeService.UpdateStoreItemAsync(id, request);
            if (!success) return NotFound("Objet non trouvé.");
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> DeleteStoreItem(Guid id)
        {
            var success = await _storeService.DeleteStoreItemAsync(id);
            if (!success) return NotFound("Objet non trouvé.");
            return NoContent();
        }
    }
}