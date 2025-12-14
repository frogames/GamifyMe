using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GamifyMe.Shared.Constants;
using GamifyMe.Api.Services;
using GamifyMe.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;

namespace GamifyMe.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly StoreService _storeService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly BadgesService _badgesService;

        public StoreController(StoreService storeService, IWebHostEnvironment webHostEnvironment, BadgesService badgesService)
        {
            _storeService = storeService;
            _webHostEnvironment = webHostEnvironment;
            _badgesService = badgesService;
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<StoreItemDto>>> GetActiveStoreItems()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var items = await _storeService.GetActiveStoreItemsAsync(userId, establishmentId);
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
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var items = await _storeService.GetAllStoreItemsSimpleListAsync(establishmentId);
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
                if (message.StartsWith("Erreur d'achat")) return StatusCode(500, message);
                return BadRequest(message);
            }

            // Trigger Badge Check
            if (establishmentId.HasValue)
            {
                _ = Task.Run(() => _badgesService.CheckAndUnlockBadgesAsync(userId, establishmentId.Value));
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

        [HttpPost("upload-image")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Aucun fichier sélectionné.");

            if (file.Length > 2 * 1024 * 1024) // 2MB limit
                return BadRequest("L'image est trop lourde (max 2Mo).");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Format non supporté (jpg, png, webp).");

            var fileName = $"product_{Guid.NewGuid()}{extension}";
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/images/products/{fileName}";
            return Ok(new { Url = url });
        }
        [HttpPost("reorder")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> ReorderStoreItems(ReorderRequestDto request)
        {
            var success = await _storeService.ReorderStoreItemsAsync(request.OrderedIds);
            if (!success) return BadRequest("Erreur lors de la réorganisation.");
            return Ok();
        }
    }
}