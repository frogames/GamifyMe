using GamifyMe.Api.Constants;
using GamifyMe.Api.Data;
using GamifyMe.Api.Services;
using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GamifyMe.Api.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur},{Roles.Gestionnaire}")]
    public class DashboardController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ObjectiveService _objectiveService;

        public DashboardController(DataContext context, ObjectiveService objectiveService)
        {
            _context = context;
            _objectiveService = objectiveService;
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

            if (!Guid.TryParse(objectiveIdString, out var objectiveId)) return BadRequest("ID Objectif invalide.");

            var objective = await _context.Objectives.FindAsync(objectiveId);
            if (objective == null) return NotFound("Objectif introuvable.");

            // 2. Récupérer le portefeuille XP et Monnaie (pour la mise à jour)
            var xpWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == user.Id && w.CurrencyCode == "XP");
            var docWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == user.Id && w.CurrencyCode == "DOC");

            // --- CHECK ACCESSIBILITY (New Requirement) ---
            if (!request.Force)
            {
                var activeObjectives = await _objectiveService.GetActiveObjectivesAsync(user.Id, establishmentId);
                var isAccessible = activeObjectives.Any(o => o.Id == objectiveId);
                
                if (!isAccessible)
                {
                    // Check if it's already validated to distinguish between "Done" and "Not Accessible"
                    var alreadyValidated = await _context.Validations.AnyAsync(v => v.UserId == user.Id && v.ObjectiveId == objectiveId);
                    
                    if (!alreadyValidated || !objective.IsUnique)
                    {
                         return Ok(new ValidationResponseDto
                         {
                             Success = false,
                             RequiresConfirmation = true,
                             Message = "Cet objectif n'est pas accessible au joueur (prérequis, dates, ou verrouillé). Voulez-vous forcer ?",
                             ScanSoundUrl = "/sounds/scan-validation.mp3"
                         });
                    }
                }
            }

            // --- 3. VÉRIFICATION DES DOUBLONS ET DE L'UNICITÉ ---

            var existingValidation = await _context.Validations
                .FirstOrDefaultAsync(v => v.UserId == user.Id && v.ObjectiveId == objectiveId);

            // 1. OBJECTIF UNIQUE (rapporte UNE SEULE FOIS, peu importe la date)
            if (objective.IsUnique && existingValidation != null)
            {
                return BadRequest(new ValidationResponseDto
                {
                    Success = false,
                    Message = $"Erreur : Cet objectif unique a déjà été validé par {user.FirstName} {user.Username}.",
                    RewardXp = 0,
                    RewardCurrency = 0
                });
            }

            // 2. OBJECTIF RÉCURRENT AVEC FRÉQUENCE (Cooldown)
            if (!objective.IsUnique && objective.FrequencyHours.HasValue)
            {
                var lastValidation = await _context.Validations
                    .Where(v => v.UserId == user.Id && v.ObjectiveId == objectiveId)
                    .OrderByDescending(v => v.Date)
                    .FirstOrDefaultAsync();

                if (lastValidation != null)
                {
                    var nextAvailableDate = lastValidation.Date.AddHours(objective.FrequencyHours.Value);
                    if (DateTime.UtcNow < nextAvailableDate)
                    {
                        var timeRemaining = nextAvailableDate - DateTime.UtcNow;
                        string timeString = timeRemaining.TotalHours >= 1 
                            ? $"{(int)timeRemaining.TotalHours}h et {timeRemaining.Minutes}min" 
                            : $"{timeRemaining.Minutes}min";

                        return BadRequest(new ValidationResponseDto
                        {
                            Success = false,
                            Message = $"Erreur : Cet objectif ne peut être validé que toutes les {objective.FrequencyHours} heures. Réessayez dans {timeString}.",
                            RewardXp = 0,
                            RewardCurrency = 0
                        });
                    }
                }
            }

            // --- 4. ATTRIBUTION DES RÉCOMPENSES (AVEC BONUS) ---

            // --- 4. ATTRIBUTION DES RÉCOMPENSES (AVEC BONUS & BOOSTS) ---

            double xpMultiplier = 1.0;
            double currencyMultiplier = 1.0;
            List<string> bonusesApplied = new List<string>();

            // A. Périodes Bonus (Globales)
            var now = DateTime.UtcNow;
            var activeBonus = await _context.BonusPeriods
                .Where(b => b.EstablishmentId == establishmentId && b.IsActive && b.StartDate <= now && b.EndDate >= now)
                .OrderByDescending(b => b.StartDate)
                .FirstOrDefaultAsync();

            if (activeBonus != null)
            {
                if (activeBonus.Type == BonusType.Xp)
                {
                    xpMultiplier *= activeBonus.Multiplier;
                    bonusesApplied.Add($"{activeBonus.Name} (XP x{activeBonus.Multiplier})");
                }
                else if (activeBonus.Type == BonusType.Currency)
                {
                    currencyMultiplier *= activeBonus.Multiplier;
                    bonusesApplied.Add($"{activeBonus.Name} (Crédits x{activeBonus.Multiplier})");
                }
            }

            // B. Boosts Joueur (Personnels)
             var userBoosts = await _context.UserInventories
                .Include(ui => ui.StoreItem)
                .Where(ui => ui.UserId == user.Id
                             && ui.IsActive
                             && (ui.ExpiresAt == null || ui.ExpiresAt > now)
                             && ui.StoreItem.ItemType == StoreItemType.Digital
                             && ui.StoreItem.DigitalActionCode != null
                             && (ui.StoreItem.DigitalActionCode.Contains("BOOST"))) 
                .ToListAsync();

            foreach (var boost in userBoosts)
            {
                var code = boost.StoreItem.DigitalActionCode!;
                // Parsing simpliste mais robuste : on cherche le multiplicateur dans le code (ex: BOOST_XP_2X)
                double boostMult = 2.0; // Par défaut
                
                // Tentative d'extraction
                var parts = code.Split('_');
                foreach (var part in parts)
                {
                    var cleanPart = part.Replace("X", "", StringComparison.OrdinalIgnoreCase);
                    if (double.TryParse(cleanPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
                    {
                         if (val > 0 && val < 20) 
                         {
                             boostMult = val;
                             break;
                         }
                    }
                }

                // Application du multiplicateur (Pour l'instant on assume que c'est du XP si le code contient XP ou par défaut)
                if (code.Contains("XP", StringComparison.OrdinalIgnoreCase))
                {
                    xpMultiplier *= boostMult;
                    bonusesApplied.Add($"Boost {boost.StoreItem.Name} (XP x{boostMult})");
                }
                // Si on avait des boosts de crédits, on ferait pareil
            }

            // C. Calcul Final
            int finalXpReward = (int)(objective.XpReward * xpMultiplier);
            int finalDocPointsReward = (int)(objective.DocPointsReward * currencyMultiplier);
            
            string bonusMessage = bonusesApplied.Any() ? " (" + string.Join(", ", bonusesApplied) + ")" : "";

            if (xpWallet != null)
            {
                xpWallet.Balance += finalXpReward;
                user.CurrentXp = (int)xpWallet.Balance;
            }
            if (docWallet != null)
            {
                docWallet.Balance += finalDocPointsReward;
                user.CurrencyBalance = (int)docWallet.Balance;
            }

            // Mise à jour de l'XP du groupe
            if (user.GroupId.HasValue)
            {
                var group = await _context.Groups.FindAsync(user.GroupId.Value);
                if (group != null)
                {
                    group.TotalXp += finalXpReward;
                }
            }

            // Mise à jour du niveau
            if (xpWallet != null)
            {
                int newLevel = 1 + ((int)xpWallet.Balance / 500);
                if (newLevel > user.Level) user.Level = newLevel;
            }
            user.LastActivityAt = DateTime.UtcNow;

            // --- 5. ENREGISTREMENT ET VALIDATION ---
            var scannerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? scannerId = Guid.TryParse(scannerIdClaim, out var sid) ? sid : null;

            var validation = new Validation
            {
                Id = Guid.NewGuid(),
                EstablishmentId = establishmentId,
                UserId = user.Id,
                ObjectiveId = objective.Id,
                Date = DateTime.UtcNow,
                ValidatedById = scannerId
            };
            _context.Validations.Add(validation);

            // Sauvegarde de : Validation, User (Level, XP, LastActivity), Wallets (Balance)
            await _context.SaveChangesAsync();

            var soundUrl = await GetUserActiveScanSoundUrl(user.Id);

            // --- 6. RETOUR AU CLIENT (Pour l'affichage des gains) ---
            return Ok(new ValidationResponseDto
            {
                Success = true,
                Message = $"Validé pour {user.FirstName} {user.Username} !{bonusMessage}",
                RewardXp = finalXpReward,
                RewardCurrency = finalDocPointsReward,
                UserNewLevel = user.Level,
                UserNewBalance = user.CurrencyBalance,
                ScanSoundUrl = soundUrl
            });
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