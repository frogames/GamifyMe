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
using System.Security.Cryptography;
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

        // POST api/users/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email.ToLower()))
            {
                return BadRequest("Cet email est déjà utilisé.");
            }

            var establishment = await _context.Establishments.FindAsync(request.EstablishmentId);
            if (establishment == null)
            {
                return BadRequest("Établissement invalide.");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                EstablishmentId = establishment.Id,
                Username = request.Username,
                Email = request.Email.ToLower(),
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = Shared.Models.UserRole.User, // Rôle par défaut
                CreatedAt = DateTime.UtcNow,
                EmailConfirmationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
                IsEmailConfirmed = false,
                QrCode = Guid.NewGuid().ToString("N") // QR Code unique
            };

            // Créer les portefeuilles (XP et Monnaie)
            var xpWallet = new Wallet { Id = Guid.NewGuid(), EstablishmentId = establishment.Id, UserId = user.Id, CurrencyCode = "XP", Balance = 0 };
            var currencyWallet = new Wallet { Id = Guid.NewGuid(), EstablishmentId = establishment.Id, UserId = user.Id, CurrencyCode = "DOC", Balance = 0 };

            _context.Users.Add(user);
            _context.Wallets.Add(xpWallet);
            _context.Wallets.Add(currencyWallet);

            await _context.SaveChangesAsync();

            // Envoyer l'email de confirmation
            await _emailService.SendConfirmationEmailAsync(user.Email, user.EmailConfirmationToken);

            return Ok("Inscription réussie. Veuillez vérifier vos emails pour confirmer votre compte.");
        }

        // POST api/users/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Login(LoginDto request)
        {
            var user = await _context.Users
                .Include(u => u.Establishment) // Inclure l'établissement
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

            if (user == null || !VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
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

        // GET api/users/confirm-email
        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
            if (user == null)
            {
                return BadRequest("Lien de confirmation invalide.");
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null; // Token utilisé
            await _context.SaveChangesAsync();

            return Ok("Email confirmé avec succès ! Vous pouvez maintenant vous connecter.");
        }

        [HttpGet("info-bar")]
        [Authorize]
        public async Task<ActionResult<InfoBarDto>> GetInfoBar()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var establishmentName = User.FindFirstValue("EstablishmentName") ?? "N/A";

            var xpWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode == "XP");
            var currencyWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode != "XP");

            int currentXp = (int)(xpWallet?.Balance ?? 0);
            int level = 1 + (currentXp / 500); // Formule simple

            return Ok(new InfoBarDto
            {
                Level = level,
                CurrencyBalance = (int)(currencyWallet?.Balance ?? 0),
                EstablishmentName = establishmentName
            });
        }

        [HttpGet("profile-details")]
        [Authorize]
        public async Task<ActionResult<UserProfileDetailsDto>> GetMyProfileDetails()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _context.Users
                .Include(u => u.Establishment)
                .Include(u => u.Wallets)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("Utilisateur introuvable.");

            var xpWallet = user.Wallets.FirstOrDefault(w => w.CurrencyCode == "XP");
            var currencyWallet = user.Wallets.FirstOrDefault(w => w.CurrencyCode != "XP");

            int currentXp = (int)(xpWallet?.Balance ?? 0);
            int currentCurrency = (int)(currencyWallet?.Balance ?? 0);
            string currencyName = currencyWallet?.CurrencyCode ?? "Points";

            int level = 1 + (currentXp / 500);
            int xpForNextLevel = level * 500;
            int xpInCurrentLevel = currentXp % 500;
            double progress = ((double)xpInCurrentLevel / 500) * 100;

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
                    Description = $"Validé : {v.Objective?.Title ?? "Objectif"}",
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
                    Description = $"Achat : {o.StoreItem?.Name ?? "Objet"}",
                    AmountChange = -((int)o.PricePaid),
                    Type = "Currency",
                    Icon = "ShoppingBag"
                });
            }

            var sortedLogs = logs.OrderByDescending(l => l.Date).Take(10).ToList();

            var dto = new UserProfileDetailsDto
            {
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                QrCode = user.QrCode,
                EstablishmentName = user.Establishment?.Name ?? "Non assigné",
                CreatedAt = user.CreatedAt,
                Level = level,
                CurrentXp = currentXp,
                XpForNextLevel = xpForNextLevel,
                ProgressPercentage = Math.Min(100, Math.Max(0, progress)),
                CurrencyBalance = currentCurrency,
                CurrencyName = currencyName,
                RecentActivity = sortedLogs
            };

            return Ok(dto);
        }

        [HttpGet("profile-scan/{qrCode}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur},{Roles.Gestionnaire}")]
        public async Task<ActionResult<ProfileScanDto>> GetProfileForScan(string qrCode)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.QrCode == qrCode);
            if (user == null) return NotFound("QR Code invalide.");

            var userProfile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                QrCode = user.QrCode,
                Status = user.IsEmailConfirmed ? "Actif" : "Non confirmé",
                EstablishmentName = (await _context.Establishments.FindAsync(user.EstablishmentId))?.Name ?? ""
            };

            var pendingOrders = await _context.Orders
                .Include(o => o.StoreItem)
                .Where(o => o.UserId == user.Id && o.Status == OrderStatus.Pending)
                .Select(o => new PendingOrderDto
                {
                    OrderId = o.Id,
                    ItemName = o.StoreItem.Name
                })
                .ToListAsync();

            return Ok(new ProfileScanDto
            {
                UserProfile = userProfile,
                PendingOrders = pendingOrders
            });
        }


        // --- Helpers (Logique privée) ---
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("EstablishmentId", user.EstablishmentId.ToString()),
                new Claim("EstablishmentName", user.Establishment.Name)
            };

            var appSettingsToken = _configuration.GetSection("Jwt:Key").Value;
            if (appSettingsToken is null)
                throw new Exception("Clé JWT non configurée.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettingsToken));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}