using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamifyMe.Shared.Constants;
using GamifyMe.Api.Services;
using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GamifyMe.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
    public class BadgesController : ControllerBase
    {
        private readonly BadgesService _badgesService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BadgesController(BadgesService badgesService, IWebHostEnvironment webHostEnvironment)
        {
            _badgesService = badgesService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<BadgeDto>>> GetAllBadges()
        {
            // Admin sees all? BadgesService.GetAllBadgesAsync relies on userId to check unlock status.
            // For admin grid, we just want the badges definitions.
            // BadgesService.GetAllBadgesAsync returns DTOs with unlock status for A USER.
            // We can pass Guid.Empty or current user.
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var badges = await _badgesService.GetAllBadgesAsync(userId, establishmentId);
            return Ok(badges);
        }

        [HttpPost]
        public async Task<ActionResult<BadgeDto>> CreateBadge(CreateBadgeDto request)
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            
            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                var imageUrl = await SaveBase64Image(request.ImageBase64);
                if (imageUrl != null) request.ImageUrl = imageUrl;
            }

            var badge = await _badgesService.CreateBadgeAsync(request, establishmentId);
            return Ok(badge);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBadge(Guid id, CreateBadgeDto request)
        {
            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                var imageUrl = await SaveBase64Image(request.ImageBase64);
                if (imageUrl != null) request.ImageUrl = imageUrl;
            }

            var success = await _badgesService.UpdateBadgeAsync(id, request);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBadge(Guid id)
        {
            var success = await _badgesService.DeleteBadgeAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Aucun fichier sélectionné.");

            if (file.Length > 2 * 1024 * 1024) 
                return BadRequest("L'image est trop lourde (max 2Mo).");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Format non supporté (jpg, png, webp).");

            var fileName = $"badge_{Guid.NewGuid()}{extension}";
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "badges");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/images/badges/{fileName}";
            return Ok(new { Url = url });
        }
        private async Task<string?> SaveBase64Image(string base64Image)
        {
            try
            {
                var parts = base64Image.Split(',');
                var data = parts.Length > 1 ? parts[1] : parts[0];
                var bytes = Convert.FromBase64String(data);
                
                var fileName = $"badge_{Guid.NewGuid()}.png";
                // Determine extension? Basic approach assumes png or just saves binary. 
                // Better: Check header. But for now simplified to .png or keeping it generic.
                // Actually, let's try to detect from header if possible or default to .png
                
                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "badges");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                
                var filePath = Path.Combine(uploadPath, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);
                
                return $"/images/badges/{fileName}";
            }
            catch
            {
                return null;
            }
        }
    }
}
