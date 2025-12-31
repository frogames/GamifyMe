using GamifyMe.Api.Data;
using GamifyMe.Api.Services;
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
    public class KitsController : ControllerBase
    {
        private readonly DataContext _context;

        public KitsController(DataContext context)
        {
            _context = context;
        }

        private Guid GetCurrentEstablishmentId()
        {
            var claim = User.FindFirst("EstablishmentId");
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
        }

        [HttpGet]
        public async Task<ActionResult<List<ContentKitDto>>> GetKits([FromQuery] KitFilterDto filter)
        {
            var query = _context.ContentKits.AsQueryable();

            if (filter.Category.HasValue)
            {
                query = query.Where(k => k.Category == filter.Category.Value);
            }

            if (filter.HasObjectives.HasValue && filter.HasObjectives.Value)
            {
                query = query.Where(k => _context.Objectives.Any(o => o.EstablishmentId == k.TemplateEstablishmentId));
            }

            if (filter.HasBadges.HasValue && filter.HasBadges.Value)
            {
                query = query.Where(k => _context.Badges.Any(b => b.EstablishmentId == k.TemplateEstablishmentId));
            }

            if (filter.HasGroups.HasValue && filter.HasGroups.Value)
            {
                query = query.Where(k => _context.Groups.Any(g => g.EstablishmentId == k.TemplateEstablishmentId));
            }
            
            if (filter.HasStoreItems.HasValue && filter.HasStoreItems.Value)
            {
                query = query.Where(k => _context.StoreItems.Any(s => s.EstablishmentId == k.TemplateEstablishmentId));
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(k => k.Name.ToLower().Contains(term) || k.Description.ToLower().Contains(term));
            }

            // Projection to ensure live data for flags
            var kits = await query.Select(k => new ContentKitDto
            {
                Id = k.Id,
                Name = k.Name,
                Description = k.Description,
                Category = k.Category,
                ImageUrl = k.ImageUrl,
                TemplateEstablishmentId = k.TemplateEstablishmentId,
                HasObjectives = _context.Objectives.Any(o => o.EstablishmentId == k.TemplateEstablishmentId),
                HasBadges = _context.Badges.Any(b => b.EstablishmentId == k.TemplateEstablishmentId),
                HasGroups = _context.Groups.Any(g => g.EstablishmentId == k.TemplateEstablishmentId),
                HasStoreItems = _context.StoreItems.Any(s => s.EstablishmentId == k.TemplateEstablishmentId),
                UsageCount = k.UsageCount,
                AverageRating = k.AverageRating
            }).ToListAsync();

            return Ok(kits);
        }

        [HttpGet("top")]
        public async Task<ActionResult<List<ContentKitDto>>> GetTopKits()
        {
            var kits = await _context.ContentKits
                .OrderByDescending(k => k.UsageCount)
                .Take(3)
                .ToListAsync();

            return Ok(kits.Select(MapToDto).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContentKitDto>> GetKit(Guid id)
        {
            var kit = await _context.ContentKits.FindAsync(id);
            if (kit == null) return NotFound("Kit introuvable.");

            var dto = MapToDto(kit);
            
            // Populate details from Template Establishment
            var templateId = kit.TemplateEstablishmentId;
            
            var objectives = await _context.Objectives.Where(o => o.EstablishmentId == templateId).ToListAsync();
            // Mapping detailed content

            
            dto.Objectives = objectives.Select(o => new ObjectiveDto
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description,
                XpReward = o.XpReward,
                DocPointsReward = o.DocPointsReward,
                Category = o.Category,
                IconName = o.IconName,
                Color = o.Color,
                AllowedValidationMethods = o.AllowedValidationMethods
            }).ToList();

            var badges = await _context.Badges.Where(b => b.EstablishmentId == templateId).ToListAsync();
            dto.Badges = badges.Select(b => new BadgeDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                IconName = b.IconName,
                Color = b.Color,
                ImageUrl = b.ImageUrl
            }).ToList();

            var groups = await _context.Groups.Where(g => g.EstablishmentId == templateId).ToListAsync();
            dto.Groups = groups.Select(g => new GroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                IconName = g.IconName,
                Color = g.Color
            }).ToList();

            var storeItems = await _context.StoreItems.Where(s => s.EstablishmentId == templateId).ToListAsync();
            dto.StoreItems = storeItems.Select(s => new StoreItemDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                ImageUrl = s.ImageUrl
            }).ToList();

            // Synch flags with actual content
            dto.HasObjectives = dto.Objectives.Any();
            dto.HasBadges = dto.Badges.Any();
            dto.HasGroups = dto.Groups.Any();
            dto.HasStoreItems = dto.StoreItems.Any();

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ContentKitDto>> CreateKit(CreateKitDto request)
        {
            // Verify Template Existence
            var template = await _context.Establishments.FindAsync(request.TemplateEstablishmentId);
            if (template == null) return BadRequest("L'établissement modèle n'existe pas.");
            if (!template.IsTemplate) return BadRequest("L'établissement sélectionné n'est pas un modèle.");

            // Auto-detect content
            bool hasObj = await _context.Objectives.AnyAsync(o => o.EstablishmentId == request.TemplateEstablishmentId);
            bool hasBadges = await _context.Badges.AnyAsync(b => b.EstablishmentId == request.TemplateEstablishmentId);
            bool hasGroups = await _context.Groups.AnyAsync(g => g.EstablishmentId == request.TemplateEstablishmentId);
            bool hasStore = await _context.StoreItems.AnyAsync(s => s.EstablishmentId == request.TemplateEstablishmentId);

            var kit = new ContentKit
            {
                Name = request.Name,
                Description = request.Description,
                Category = request.Category,
                ImageUrl = request.ImageUrl,
                TemplateEstablishmentId = request.TemplateEstablishmentId,
                HasObjectives = hasObj,
                HasBadges = hasBadges,
                HasGroups = hasGroups,
                HasStoreItems = hasStore,
                UsageCount = 0,
                AverageRating = 0
            };

            _context.ContentKits.Add(kit);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetKit), new { id = kit.Id }, MapToDto(kit));
        }
        
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateKit(Guid id, UpdateKitDto request)
        {
            if (id != request.Id) return BadRequest();
            
            var kit = await _context.ContentKits.FindAsync(id);
            if (kit == null) return NotFound();

            kit.Name = request.Name;
            kit.Description = request.Description;
            kit.Category = request.Category;
            kit.ImageUrl = request.ImageUrl;

            // Recalculate flags if template changed? Not implemented for now.
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/rate")]
        [Authorize(Roles = "SuperAdmin,Admin,Editeur")]
        public async Task<IActionResult> RateKit(Guid id, RateKitDto request)
        {
            var establishmentId = GetCurrentEstablishmentId();
            if (establishmentId == Guid.Empty) return Unauthorized();

            var kit = await _context.ContentKits.FindAsync(id);
            if (kit == null) return NotFound("Kit introuvable.");

            var existingRating = await _context.KitRatings
                .FirstOrDefaultAsync(r => r.KitId == id && r.EstablishmentId == establishmentId);

            if (existingRating != null)
            {
                existingRating.Rating = request.Rating;
                existingRating.Comment = request.Comment;
            }
            else
            {
                var rating = new KitRating
                {
                    KitId = id,
                    EstablishmentId = establishmentId,
                    Rating = request.Rating,
                    Comment = request.Comment
                };
                _context.KitRatings.Add(rating);
            }

            await _context.SaveChangesAsync();

            // Update Average
            var avg = await _context.KitRatings.Where(r => r.KitId == id).AverageAsync(r => r.Rating);
            kit.AverageRating = avg;
            await _context.SaveChangesAsync();

            return Ok(new { AverageRating = avg });
        }

        [HttpPost("{id}/import")]
        [Authorize(Roles = "SuperAdmin,Admin,Editeur")]
        public async Task<ActionResult<KitImportResultDto>> ImportKit(Guid id, [FromServices] ContentImportService importService)
        {
            var targetId = GetCurrentEstablishmentId();
            if (targetId == Guid.Empty) return Unauthorized();

            var kit = await _context.ContentKits.FindAsync(id);
            if (kit == null) return NotFound("Kit introuvable.");

            try 
            {
                // We delegate to the existing import service, but using the TemplateEstablishmentId from the Kit
                var result = await importService.ImportKitAsync(kit.TemplateEstablishmentId, targetId);
                
                // Increment Usage
                kit.UsageCount++;
                await _context.SaveChangesAsync();

                return Ok(result);
            } 
            catch (Exception ex) 
            {
                 return BadRequest($"Erreur lors de l'import: {ex.Message}");
            }
        }
        
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteKit(Guid id)
        {
            var kit = await _context.ContentKits.FindAsync(id);
            if (kit == null) return NotFound();

            // Check if used? 
            // We can delete the kit definition, but maybe keep it if it has usage?
            // For now, allow delete.
            
            _context.ContentKits.Remove(kit);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("upload-image")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromServices] IWebHostEnvironment env)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Aucun fichier sélectionné.");

            if (file.Length > 2 * 1024 * 1024) // 2MB limit
                return BadRequest("L'image est trop lourde (max 2Mo).");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Format non supporté (jpg, png, webp).");

            var fileName = $"kit_{Guid.NewGuid()}{extension}";
            var uploadPath = Path.Combine(env.WebRootPath, "images", "kits");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/images/kits/{fileName}";
            return Ok(new { Url = url });
        }

        private static ContentKitDto MapToDto(ContentKit kit)
        {
            return new ContentKitDto
            {
                Id = kit.Id,
                Name = kit.Name,
                Description = kit.Description,
                Category = kit.Category,
                ImageUrl = kit.ImageUrl,
                TemplateEstablishmentId = kit.TemplateEstablishmentId,
                HasObjectives = kit.HasObjectives,
                HasBadges = kit.HasBadges,
                HasGroups = kit.HasGroups,
                HasStoreItems = kit.HasStoreItems,
                UsageCount = kit.UsageCount,
                AverageRating = kit.AverageRating
            };
        }
    }
}
