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
            Response.Headers.Append("Access-Control-Allow-Origin", "*");
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
                Username = request.Username,
                FirstName = request.FirstName,
                Email = request.Email.ToLower(),
                PasswordHash = passwordHash,
                Role = Roles.User,
                EstablishmentId = request.EstablishmentId,
                QrCode = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                IsEmailConfirmed = false,
                EmailConfirmationToken = Guid.NewGuid().ToString()
            };

            var xpWallet = new Wallet { Id = Guid.NewGuid(), EstablishmentId = establishment.Id, UserId = user.Id, CurrencyCode = "XP", Balance = 0 };
            var currencyWallet = new Wallet { Id = Guid.NewGuid(), EstablishmentId = establishment.Id, UserId = user.Id, CurrencyCode = "DOC", Balance = 0 };

            _context.Users.Add(user);
            _context.Wallets.Add(xpWallet);
            _context.Wallets.Add(currencyWallet);

            await _context.SaveChangesAsync();

            // Envoyer l'email de confirmation
            if (!user.IsEmailConfirmed)
            {
                try 
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
                catch (Exception ex)
                {
                    Console.WriteLine($"[Register] Error sending email: {ex.Message}");
                    return Ok("Compte créé avec succès, mais l'envoi de l'email de confirmation a échoué. Veuillez contacter le support.");
                }
            }

            return Ok("Compte créé avec succès. Vous pouvez vous connecter.");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Login(LoginRequest request)
        {
            Response.Headers.Append("Access-Control-Allow-Origin", "*");

            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("L'email est requis.");
            }

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

        [HttpGet("establishment-name/{establishmentId}")]
        [AllowAnonymous]
        public async Task<ActionResult<EstablishmentNameDto>> GetEstablishmentName(Guid establishmentId)
        {
            var establishment = await _context.Establishments.FindAsync(establishmentId);
            if (establishment == null) return NotFound("Établissement introuvable.");
            return Ok(new EstablishmentNameDto { Name = establishment.Name });
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
                EstablishmentName = User.FindFirstValue("EstablishmentName") ?? "N/A",
                FirstName = User.FindFirstValue("FirstName") ?? ""
            });
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<UserProfileDetailsDto>> UpdateProfile(UpdateUserProfileDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Utilisateur introuvable.");

            // Check uniqueness for Username and Email (excluding current user)
            if (await _context.Users.AnyAsync(u => u.Id != userId && u.Username == request.Username))
            {
                return BadRequest("Ce nom d'utilisateur est déjà pris.");
            }

            if (await _context.Users.AnyAsync(u => u.Id != userId && u.Email == request.Email.ToLower()))
            {
                return BadRequest("Cet email est déjà utilisé.");
            }

            user.FirstName = request.FirstName;
            user.Username = request.Username;
            user.Email = request.Email.ToLower();

            await _context.SaveChangesAsync();

            return await GetMyProfileDetails();
        }

        [HttpGet("profile-details")]
        [Authorize]
        public async Task<ActionResult<UserProfileDetailsDto>> GetMyProfileDetails()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            Console.WriteLine($"[GetMyProfileDetails] Requesting profile for UserId: {userId}");

            var user = await _context.Users
                .Include(u => u.Establishment)
                .Include(u => u.Wallets)
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                Console.WriteLine($"[GetMyProfileDetails] User not found for UserId: {userId}");
                return NotFound("Utilisateur introuvable.");
            }

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

            // Fetch Active UI Theme
            var activeThemeItem = await _context.UserInventories
                .Include(ui => ui.StoreItem)
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.IsActive && ui.StoreItem.DigitalActionCode != null && ui.StoreItem.DigitalActionCode.StartsWith("UI_THEME_"));
            
            string activeTheme = activeThemeItem?.StoreItem.DigitalActionCode ?? GamifyMe.Shared.Constants.ThemeConstants.Default;

            // Fetch Active QR Style
            var activeQrItem = await _context.UserInventories
                .Include(ui => ui.StoreItem)
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.IsActive && ui.StoreItem.DigitalActionCode != null && ui.StoreItem.DigitalActionCode.StartsWith("QR_STYLE_"));

            string activeQrStyle = activeQrItem?.StoreItem.DigitalActionCode ?? GamifyMe.Shared.Constants.ThemeConstants.QrStyleDefault;

            // Fetch Active XP Boost
            var activeBoostItem = await _context.UserInventories
                .Include(ui => ui.StoreItem)
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.IsActive && ui.StoreItem.DigitalActionCode != null && ui.StoreItem.DigitalActionCode.Contains("BOOST") && (ui.ExpiresAt == null || ui.ExpiresAt > DateTime.UtcNow));
            
            int activeBoostMultiplier = activeBoostItem != null ? 2 : 1;
            DateTime? boostEndsAt = activeBoostItem?.ExpiresAt;

            // Calculate Rank
            var establishmentUserIds = await _context.Users
                .Where(u => u.EstablishmentId == user.EstablishmentId)
                .Select(u => u.Id)
                .ToListAsync();

            var userXpBalances = await _context.Wallets
                .Where(w => establishmentUserIds.Contains(w.UserId) && w.CurrencyCode == "XP")
                .Select(w => new { w.UserId, w.Balance })
                .ToListAsync();

            var rankedUsers = userXpBalances
                .OrderByDescending(w => w.Balance)
                .Select((w, index) => new { w.UserId, Rank = index + 1 })
                .ToList();

            var userRank = rankedUsers.FirstOrDefault(r => r.UserId == userId)?.Rank ?? 0;

            return Ok(new UserProfileDetailsDto
            {
                Username = user.Username,
                FirstName = user.FirstName,
                Email = user.Email,
                Role = user.Role,
                QrCode = user.QrCode,
                EstablishmentName = user.Establishment?.Name ?? "N/A",
                CreatedAt = user.CreatedAt,
                Level = level,
                CurrentXp = currentXp,
                XpForNextLevel = level * 500,
                ProgressPercentage = Math.Min(100, Math.Max(0, ((double)(currentXp % 500) / 500) * 100)),
                CurrencyBalance = currentCurrency,
                CurrencyName = currencyWallet?.CurrencyCode ?? "Points",
                GroupId = user.GroupId,
                GroupName = user.Group?.Name,
                RecentActivity = sortedLogs,
                ActiveUiTheme = activeTheme,
                ActiveQrCodeStyle = activeQrStyle,
                ActiveBoostMultiplier = activeBoostMultiplier,
                BoostEndsAt = boostEndsAt,
                Rank = userRank
            });
        }

        [HttpPost("inventory/{userInventoryId}/toggle")]
        [Authorize]
        public async Task<IActionResult> ToggleInventoryItem(Guid userInventoryId)
        {
            var userId = GetCurrentUserId();
            var userInventory = await _context.UserInventories
                .Include(ui => ui.StoreItem)
                .FirstOrDefaultAsync(ui => ui.Id == userInventoryId && ui.UserId == userId);

            if (userInventory == null) return NotFound("Objet non trouvé.");

            // Toggle logic
            if (userInventory.IsActive)
            {
                // Deactivate
                userInventory.IsActive = false;
            }
            else
            {
                // Activate
                // First, deactivate other items of the same "type" to ensure mutual exclusivity
                var code = userInventory.StoreItem.DigitalActionCode;
                if (!string.IsNullOrEmpty(code))
                {
                    // We need to fetch potential conflicts. 
                    // Note: We can't easily do "StartsWith" on the joined table in a single update without fetching.
                    // So we fetch active items that might conflict.
                    
                    var potentialConflicts = await _context.UserInventories
                        .Include(ui => ui.StoreItem)
                        .Where(ui => ui.UserId == userId && ui.IsActive && ui.Id != userInventoryId)
                        .ToListAsync();

                    foreach (var other in potentialConflicts)
                    {
                        var otherCode = other.StoreItem.DigitalActionCode;
                        if (string.IsNullOrEmpty(otherCode)) continue;

                        bool isConflict = false;

                        if (code.StartsWith("UI_THEME_") && otherCode.StartsWith("UI_THEME_")) isConflict = true;
                        else if (code.StartsWith("QR_STYLE_") && otherCode.StartsWith("QR_STYLE_")) isConflict = true;
                        else if (code == "SCAN_SOUND" && otherCode == "SCAN_SOUND") isConflict = true;

                        if (isConflict)
                        {
                            other.IsActive = false;
                        }
                    }
                }

                userInventory.IsActive = true;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("inventory")]
        public async Task<ActionResult<List<UserInventoryDto>>> GetMyInventory()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var inventoryItems = await _context.UserInventories
                .Include(ui => ui.StoreItem)
                .Where(ui => ui.UserId == userId)
                .Where(ui => ui.ExpiresAt == null || ui.ExpiresAt > DateTime.UtcNow) // Filter out expired items
                .OrderByDescending(ui => ui.DateAcquired)
                .Select(ui => new UserInventoryDto
                {
                    Id = ui.Id,
                    ItemName = ui.StoreItem.Name,
                    Description = ui.StoreItem.Description,
                    IconName = ui.StoreItem.IconName,
                    AcquiredDate = ui.DateAcquired,
                    IsActive = ui.IsActive,
                    ExpiresAt = ui.ExpiresAt,
                    ItemType = ui.StoreItem.ItemType.ToString(),
                    DigitalActionCode = ui.StoreItem.DigitalActionCode,
                    DigitalAssetUrl = ui.StoreItem.DigitalAssetUrl
                })
                .ToListAsync();

            return Ok(inventoryItems);
        }

        [HttpGet("profile-scan/{qrCode}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur},{Roles.Gestionnaire}")]
        public async Task<ActionResult<ProfileScanDto>> GetProfileForScan(string qrCode)
        {
            Console.WriteLine($"[GetProfileForScan] Scanning QR: {qrCode}");

            var user = await _context.Users
                .Include(u => u.Establishment)
                .FirstOrDefaultAsync(u => u.QrCode == qrCode);

            if (user == null) 
            {
                Console.WriteLine("[GetProfileForScan] User not found.");
                return NotFound("QR Code invalide.");
            }

            Console.WriteLine($"[GetProfileForScan] User found: {user.Username} ({user.Id})");

            // Fetch ALL orders for this user
            var allOrders = await _context.Orders
                .Include(o => o.StoreItem)
                .Where(o => o.UserId == user.Id)
                .ToListAsync();

            Console.WriteLine($"[GetProfileForScan] Found {allOrders.Count} total orders for user.");

            var pendingOrders = new List<PendingOrderDto>();

            foreach (var o in allOrders)
            {
                bool isPhysical = o.StoreItem.ItemType == StoreItemType.Physical;
                bool isPending = o.Status == OrderStatus.Pending;
                
                // Detection of bugged orders: Physical items that were auto-completed at purchase time
                // We check if DateCompleted is very close to DatePurchased (e.g. within 10 seconds)
                // This allows us to show these items as "Pending" to the admin so they can be validated.
                bool isBuggedCompleted = o.Status == OrderStatus.Completed 
                                         && isPhysical 
                                         && o.DateCompleted.HasValue 
                                         && Math.Abs((o.DateCompleted.Value - o.DatePurchased).TotalSeconds) < 10;

                if (isPhysical && (isPending || isBuggedCompleted))
                {
                    pendingOrders.Add(new PendingOrderDto
                    {
                        OrderId = o.Id,
                        ItemName = o.StoreItem.Name,
                        ItemIcon = o.StoreItem.IconName,
                        DatePurchased = o.DatePurchased
                    });
                }
            }
            return Ok(new ProfileScanDto
            {
                UserProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
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
                new Claim("FirstName", user.FirstName ?? ""),
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