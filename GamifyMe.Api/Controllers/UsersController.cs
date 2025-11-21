using GamifyMe.Api.Constants;
using GamifyMe.Api.Data;
using GamifyMe.Api.Services;
using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GamifyMe.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UsersController(DataContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Register(RegisterDto request)
        {
            var establishment = await _context.Establishments.FindAsync(request.EstablishmentId);
            if (establishment == null)
            {
                return BadRequest("Établissement invalide.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email.ToLower()))
            {
                return BadRequest("Cet email est déjà utilisé.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                EstablishmentId = establishment.Id,
                Username = request.Username,
                Email = request.Email.ToLower(),
                PasswordHash = passwordHash,
                Role = Roles.User,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmationToken = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64)),
                IsEmailConfirmed = false,
                QrCode = Guid.NewGuid().ToString("N")
            };

            if (user.Email.Contains("admin") || user.Email.Contains("cx6"))
            {
                user.Role = Roles.SuperAdmin;
                user.IsEmailConfirmed = true;
            }

            var xpWallet = new Wallet { Id = Guid.NewGuid(), EstablishmentId = establishment.Id, UserId = user.Id, CurrencyCode = "XP", Balance = 0 };
            var currencyWallet = new Wallet { Id = Guid.NewGuid(), EstablishmentId = establishment.Id, UserId = user.Id, CurrencyCode = "DOC", Balance = 0 };

            _context.Users.Add(user);
            _context.Wallets.Add(xpWallet);
            _context.Wallets.Add(currencyWallet);

            await _context.SaveChangesAsync();

            // Envoyer l'email de confirmation
            if (!user.IsEmailConfirmed)
            {
                var appUrl = _configuration["AppUrl"];
                var confirmationLink = $"{appUrl}/confirm-email?token={user.EmailConfirmationToken}";
                var subject = "Confirmez votre compte GamifyMe";
                var body = $@"
                    <h1>Bienvenue sur GamifyMe !</h1>
                    <p>Merci de vous être inscrit. Veuillez cliquer sur le lien ci-dessous pour confirmer votre adresse email :</p>
                    <p><a href='{confirmationLink}'>Confirmer mon email</a></p>
                    <p>Si le lien ne fonctionne pas, copiez-collez l'URL suivante dans votre navigateur : {confirmationLink}</p>";

                await _emailService.SendEmailAsync(user.Email, subject, body);
                return Ok("Compte créé avec succès. Veuillez vérifier vos emails pour confirmer votre compte.");
            }

            return Ok("Compte créé avec succès. Vous pouvez vous connecter.");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Login(LoginDto request)
        {
            var user = await _context.Users
                .Include(u => u.Establishment)
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

            if (user == null)
            {
                return BadRequest("Email ou mot de passe incorrect.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Email ou mot de passe incorrect.");
            }

            if (!user.IsEmailConfirmed)
            {
                return BadRequest("Veuillez confirmer votre email avant de vous connecter.");
            }

            string token = CreateToken(user);
            return Ok(token);
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
            if (user == null) return BadRequest("Lien invalide.");
            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            await _context.SaveChangesAsync();
            return Ok("Email confirmé.");
        }

        [HttpGet("info-bar")]
        [Authorize]
        public async Task<ActionResult<InfoBarDto>> GetInfoBar()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var xpWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode == "XP");
            var otherWallets = await _context.Wallets.Where(w => w.UserId == userId && w.CurrencyCode != "XP")
                .Select(w => new WalletBalanceDto { CurrencyCode = w.CurrencyCode, Balance = (int)w.Balance }).ToListAsync();

            int currentXp = (int)(xpWallet?.Balance ?? 0);
            int level = 1 + (currentXp / 500);

            return Ok(new InfoBarDto
            {
                Level = level,
                CurrentXp = currentXp,
                XpToNextLevel = level * 500,
                OtherWallets = otherWallets,
                EstablishmentName = User.FindFirstValue("EstablishmentName") ?? "N/A"
            });
        }

        [HttpGet("profile-details")]
        [Authorize]
        public async Task<ActionResult<UserProfileDetailsDto>> GetMyProfileDetails()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Establishment)
                .Include(u => u.Wallets)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("Utilisateur introuvable.");

            var xpWallet = user.Wallets.FirstOrDefault(w => w.CurrencyCode == "XP");
            var currencyWallet = user.Wallets.FirstOrDefault(w => w.CurrencyCode != "XP");
            int currentXp = (int)(xpWallet?.Balance ?? 0);
            int currentCurrency = (int)(currencyWallet?.Balance ?? 0);
            int level = 1 + (currentXp / 500);

            var logs = new List<UserActivityLogDto>();

            var validations = await _context.Validations
                .Include(v => v.Objective)
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.Date)
                .Take(10)
                .ToListAsync();

            foreach (var v in validations)
            {
                logs.Add(new UserActivityLogDto
                {
                    Date = v.Date,
                    Description = v.Objective != null ? $"Validé : {v.Objective.Title}" : "Objectif supprimé",
                    AmountChange = v.Objective?.XpReward ?? 0,
                    Type = "XP",
                    Icon = v.Objective?.IconName ?? "Check"
                });
            }

            var orders = await _context.Orders
                .Include(o => o.StoreItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.DatePurchased)
                .Take(10)
                .ToListAsync();

            foreach (var o in orders)
            {
                logs.Add(new UserActivityLogDto
                {
                    Date = o.DatePurchased,
                    Description = $"Achat : {o.StoreItem?.Name ?? "Objet supprimé"}",
                    AmountChange = -o.PricePaid,
                    Type = "Currency",
                    Icon = "ShoppingBag"
                });
            }

            var sortedLogs = logs.OrderByDescending(l => l.Date).Take(10).ToList();

            return Ok(new UserProfileDetailsDto
            {
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                QrCode = user.QrCode,
                EstablishmentName = user.Establishment?.Name ?? "Non assigné",
                CreatedAt = user.CreatedAt,
                Level = level,
                CurrentXp = currentXp,
                XpForNextLevel = level * 500,
                ProgressPercentage = Math.Min(100, Math.Max(0, ((double)(currentXp % 500) / 500) * 100)),
                CurrencyBalance = currentCurrency,
                CurrencyName = currencyWallet?.CurrencyCode ?? "Points",
                RecentActivity = sortedLogs
            });
        }

        [HttpGet("inventory")]
        [Authorize]
        public async Task<ActionResult<List<UserInventoryDto>>> GetMyInventory()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var inventoryItems = await _context.UserInventories
                .Include(ui => ui.StoreItem)
                .Where(ui => ui.UserId == userId)
                .OrderByDescending(ui => ui.DateAcquired)
                .Select(ui => new UserInventoryDto
                {
                    Id = ui.Id,
                    ItemName = ui.StoreItem.Name,
                    Description = ui.StoreItem.Description,
                    IconName = ui.StoreItem.IconName,
                    AcquiredDate = ui.DateAcquired,
                    IsUsed = ui.IsActive,
                    UsedDate = ui.ExpiresAt,
                    ItemType = ui.StoreItem.ItemType.ToString()
                })
                .ToListAsync();

            return Ok(inventoryItems);
        }

        [HttpGet("profile-scan/{qrCode}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur},{Roles.Gestionnaire}")]
        public async Task<ActionResult<ProfileScanDto>> GetProfileForScan(string qrCode)
        {
            var user = await _context.Users
                .Include(u => u.Establishment)
                .FirstOrDefaultAsync(u => u.QrCode == qrCode);

            if (user == null) return NotFound("QR Code invalide.");

            var pendingOrders = await _context.Orders
                .Include(o => o.StoreItem)
                .Where(o => o.UserId == user.Id && o.Status == OrderStatus.Pending && o.StoreItem.ItemType == StoreItemType.Physical)
                .Select(o => new PendingOrderDto
                {
                    OrderId = o.Id,
                    ItemName = o.StoreItem.Name,
                    ItemIcon = o.StoreItem.IconName,
                    DatePurchased = o.DatePurchased
                })
                .ToListAsync();

            return Ok(new ProfileScanDto
            {
                UserProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    EstablishmentName = user.Establishment?.Name ?? "N/A",
                    QrCode = user.QrCode,
                    CreatedAt = user.CreatedAt
                },
                PendingOrders = pendingOrders
            });
        }

        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("EstablishmentId", user.EstablishmentId.ToString()),
                new Claim("EstablishmentName", user.Establishment.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}