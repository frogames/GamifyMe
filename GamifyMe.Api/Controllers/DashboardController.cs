using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamifyMe.Shared.Constants;
using GamifyMe.Api.Data;
using GamifyMe.Api.Services;
using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using GamifyMe.Shared.Helpers;

namespace GamifyMe.Api.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Coach},{Roles.Staff}")]
    public class DashboardController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ObjectiveService _objectiveService;
        private readonly BadgesService _badgesService;

        public DashboardController(DataContext context, ObjectiveService objectiveService, BadgesService badgesService)
        {
            _context = context;
            _objectiveService = objectiveService;
            _badgesService = badgesService;
        }

        [HttpGet("activity-logs")]
        public async Task<ActionResult<List<DashboardLogDto>>> GetDashboardLogs()
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var logs = new List<DashboardLogDto>();

            // 1. SCANS (Sécurisé)
            var validationLogs = await _context.Validations
                .Include(v => v.Objective)
                .Include(v => v.User)
                .Include(v => v.ValidatedBy)
                .Where(v => v.EstablishmentId == establishmentId)
                .OrderByDescending(v => v.Date)
                .Take(50)
                .ToListAsync();

            logs.AddRange(validationLogs.Select(v => new DashboardLogDto
            {
                Date = v.Date,
                ActorName = $"{v.User.FirstName} {v.User.Username}",
                ActionType = "Scan",
                Details = $"Objectif : {v.Objective.Title}",
                Icon = "QrCodeScanner",
                Color = "Success",
                ScannerName = v.ValidatedBy != null ? $"{v.ValidatedBy.FirstName} {v.ValidatedBy.Username}" : "Système",
                ScannedUserName = $"{v.User.FirstName} {v.User.Username}"
            }));

            // 2. ACHATS (Sécurisé)
            var orderLogs = await _context.Orders
                .Include(o => o.StoreItem)
                .Include(o => o.User)
                .Where(o => o.EstablishmentId == establishmentId)
                .OrderByDescending(o => o.DatePurchased)
                .Take(50)
                .ToListAsync();

            logs.AddRange(orderLogs.Select(o => new DashboardLogDto
            {
                Date = o.DatePurchased,
                ActorName = $"{o.User.FirstName} {o.User.Username}",
                ActionType = "Achat",
                Details = $"Article : {o.StoreItem.Name}",
                Icon = "ShoppingCart",
                Color = "Info"
            }));

            return Ok(logs.OrderByDescending(l => l.Date).Take(50).ToList());
        }

        // POST api/dashboard/process-scan
        [HttpPost("process-scan")]
        public async Task<ActionResult<ValidationResponseDto>> ProcessScan([FromBody] CreateValidationDto request)
        {
            Console.WriteLine($"[API] Reçu ProcessScan : Type={request.Type}, QR={request.UserQrCode}");

            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);

            if (request.Type == "Objective")
            {
                return await ProcessObjectiveScan(request, establishmentId);
            }
            else if (request.Type == "Profile")
            {
                return await ProcessProfileScan(request.UserQrCode, establishmentId);
            }

            return BadRequest("Type inconnu");
        }

        private async Task<ActionResult<ValidationResponseDto>> ProcessProfileScan(string userQrCode, Guid establishmentId)
        {
            // 1. Trouver l'utilisateur
            var user = await _context.Users.FirstOrDefaultAsync(u => u.QrCode == userQrCode);
            if (user == null) return NotFound("Joueur introuvable (QR Code invalide).");
            if (user.EstablishmentId != establishmentId) return NotFound("Ce joueur appartient à un autre établissement.");

            // 2. Récupérer les commandes en attente (Click & Collect ou Digital non livré)
            var pendingOrdersCount = await _context.Orders
                .Where(o => o.UserId == user.Id && o.EstablishmentId == establishmentId && o.Status == OrderStatus.Pending)
                .CountAsync();

            string msg;
            if (pendingOrdersCount == 0)
            {
                msg = $"Profil de {user.FirstName} {user.Username} scanné. Aucune commande en attente.";
            }
            else
            {
                msg = $"Profil de {user.FirstName} {user.Username} scanné. {pendingOrdersCount} commande(s) en attente de validation.";
            }

            var soundUrl = await GetUserActiveScanSoundUrl(user.Id);
            return Ok(new ValidationResponseDto
            {
                Success = true,
                Message = msg,
                ScanSoundUrl = soundUrl
            });
        }

        private async Task<ActionResult<ValidationResponseDto>> ProcessObjectiveScan(CreateValidationDto request, Guid establishmentId)
        {
            var objectiveIdString = request.QrCode;
            var userQrCode = request.UserQrCode;

            // 1. Trouver l'utilisateur et l'objectif
            var user = await _context.Users.FirstOrDefaultAsync(u => u.QrCode == userQrCode);
            if (user == null) return NotFound("Joueur introuvable (QR Code invalide).");
            if (user.EstablishmentId != establishmentId) return NotFound("Ce joueur appartient à un autre établissement.");

            if (!Guid.TryParse(objectiveIdString, out var objectiveId)) return BadRequest("ID Objectif invalide.");

            var objective = await _context.Objectives.FindAsync(objectiveId);
            if (objective == null) return NotFound("Objectif introuvable.");
            if (objective.EstablishmentId != establishmentId) return NotFound("Objectif introuvable dans cet établissement.");

            var scannerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? scannerId = Guid.TryParse(scannerIdClaim, out var sid) ? sid : null;

            // Call Service
            var result = await _objectiveService.ValidateObjectiveAsync(user.Id, objective.Id, scannerId, request.Force, establishmentId: establishmentId);

            if (!result.Success)
            {
                if (result.RequiresConfirmation)
                {
                    result.ScanSoundUrl = "/sounds/scan-validation.mp3";
                     return Ok(result);
                }
                return BadRequest(result);
            }

            // --- 8. CHECK BADGES UNLOCK ---
            try
            {
                await _badgesService.CheckAndUnlockBadgesAsync(user.Id, establishmentId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DashboardController] Error checking badges: {ex.Message}");
            }

            // Get Sound
            var soundUrl = await GetUserActiveScanSoundUrl(user.Id);
            result.ScanSoundUrl = soundUrl;

            return Ok(result);
        }


        private async Task<string?> GetUserActiveScanSoundUrl(Guid userId)
        {
            var inventoryItem = await _context.UserInventories
                .Include(ui => ui.StoreItem)
                .Where(ui => ui.UserId == userId && ui.IsActive && ui.StoreItem.DigitalActionCode == "SCAN_SOUND")
                .FirstOrDefaultAsync();
            
            return inventoryItem?.StoreItem.DigitalAssetUrl;
        }
    }
}