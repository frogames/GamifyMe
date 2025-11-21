using GamifyMe.Api.Constants;
using GamifyMe.Api.Data;
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

        public DashboardController(DataContext context)
        {
            _context = context;
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
                .Where(v => v.EstablishmentId == establishmentId)
                .OrderByDescending(v => v.Date)
                .Take(20)
                .Select(v => new DashboardLogDto
                {
                    Date = v.Date,
                    ActorName = v.User != null ? v.User.Username : "Utilisateur inconnu",
                    ActionType = "Validation Objectif",
                    Details = v.Objective != null ? v.Objective.Title : "Objectif supprimé",
                    Icon = "QrCode",
                    Color = "Success"
                })
                .ToListAsync();
            logs.AddRange(validationLogs);

            // 2. ACHATS (Sécurisé)
            var orderLogs = await _context.Orders
                .Include(o => o.StoreItem)
                .Include(o => o.User)
                .Where(o => o.StoreItem.EstablishmentId == establishmentId && o.Status == OrderStatus.Completed)
                .OrderByDescending(o => o.DateCompleted)
                .Take(20)
                .Select(o => new DashboardLogDto
                {
                    Date = o.DateCompleted ?? o.DatePurchased,
                    ActorName = o.User != null ? o.User.Username : "Utilisateur inconnu",
                    ActionType = "Achat Boutique",
                    Details = o.StoreItem != null ? o.StoreItem.Name : "Objet supprimé",
                    Icon = "ShoppingBag",
                    Color = "Info"
                })
                .ToListAsync();
            logs.AddRange(orderLogs);

            return Ok(logs.OrderByDescending(l => l.Date).Take(20).ToList());
        }

        // POST api/dashboard/process-scan
        [HttpPost("process-scan")]
        public async Task<ActionResult<ValidationResponseDto>> ProcessScan([FromBody] CreateValidationDto request)
        {
            Console.WriteLine($"[API] Reçu ProcessScan : Type={request.Type}, QR={request.UserQrCode}");

            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);

            if (request.Type == "Objective")
            {
                return await ProcessObjectiveScan(request.QrCode, request.UserQrCode, establishmentId);
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
            var pendingOrders = await _context.Orders
                .Include(o => o.StoreItem)
                .Where(o => o.UserId == user.Id && o.EstablishmentId == establishmentId && o.Status == OrderStatus.Pending)
                .ToListAsync();

            if (!pendingOrders.Any())
            {
                return Ok(new ValidationResponseDto
                {
                    Success = true,
                    Message = $"Profil de {user.Username} scanné. Aucune commande en attente."
                });
            }

            int validatedCount = 0;
            var validatedItemNames = new List<string>();

            foreach (var order in pendingOrders)
            {
                // Valider la commande
                order.Status = OrderStatus.Completed;
                order.DateCompleted = DateTime.UtcNow;
                validatedCount++;

                validatedItemNames.Add(order.StoreItem.Name);
            }

            await _context.SaveChangesAsync();

            string msg = $"{validatedCount} commande(s) validée(s) pour {user.Username}.";
            if (validatedItemNames.Any())
            {
                msg += $" Articles : {string.Join(", ", validatedItemNames)}.";
            }

            return Ok(new ValidationResponseDto
            {
                Success = true,
                Message = msg
            });
        }

        private async Task<ActionResult<ValidationResponseDto>> ProcessObjectiveScan(string objectiveIdString, string userQrCode, Guid establishmentId)
        {
            // 1. Trouver l'utilisateur et l'objectif
            var user = await _context.Users.FirstOrDefaultAsync(u => u.QrCode == userQrCode);
            if (user == null) return NotFound("Joueur introuvable (QR Code invalide).");

            if (!Guid.TryParse(objectiveIdString, out var objectiveId)) return BadRequest("ID Objectif invalide.");

            var objective = await _context.Objectives.FindAsync(objectiveId);
            if (objective == null) return NotFound("Objectif introuvable.");

            // 2. Récupérer le portefeuille XP et Monnaie (pour la mise à jour)
            var xpWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == user.Id && w.CurrencyCode == "XP");
            var docWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == user.Id && w.CurrencyCode == "DOC");

            // --- 3. VÉRIFICATION DES DOUBLONS ET DE L'UNICITÉ ---

            var existingValidation = await _context.Validations
                .FirstOrDefaultAsync(v => v.UserId == user.Id && v.ObjectiveId == objectiveId);

            // 1. OBJECTIF UNIQUE (rapporte UNE SEULE FOIS, peu importe la date)
            if (objective.IsUnique && existingValidation != null)
            {
                return BadRequest(new ValidationResponseDto
                {
                    Success = false,
                    Message = $"Erreur : Cet objectif unique a déjà été validé par {user.Username}.",
                    RewardXp = 0,
                    RewardCurrency = 0
                });
            }

            // --- 4. ATTRIBUTION DES RÉCOMPENSES ---

            if (xpWallet != null)
            {
                xpWallet.Balance += objective.XpReward;
                user.CurrentXp = (int)xpWallet.Balance;
            }
            if (docWallet != null)
            {
                docWallet.Balance += objective.DocPointsReward;
                user.CurrencyBalance = (int)docWallet.Balance;
            }

            // Mise à jour du niveau
            if (xpWallet != null)
            {
                int newLevel = 1 + ((int)xpWallet.Balance / 500);
                if (newLevel > user.Level) user.Level = newLevel;
            }
            user.LastActivityAt = DateTime.UtcNow;

            // --- 5. ENREGISTREMENT ET VALIDATION ---
            var validation = new Validation
            {
                Id = Guid.NewGuid(),
                EstablishmentId = establishmentId,
                UserId = user.Id,
                ObjectiveId = objective.Id,
                Date = DateTime.UtcNow
            };
            _context.Validations.Add(validation);

            // Sauvegarde de : Validation, User (Level, XP, LastActivity), Wallets (Balance)
            await _context.SaveChangesAsync();

            // --- 6. RETOUR AU CLIENT (Pour l'affichage des gains) ---
            return Ok(new ValidationResponseDto
            {
                Success = true,
                Message = $"Validé pour {user.Username} !",
                RewardXp = objective.XpReward,
                RewardCurrency = objective.DocPointsReward,
                UserNewLevel = user.Level,
                UserNewBalance = user.CurrencyBalance
            });
        }
    }
}