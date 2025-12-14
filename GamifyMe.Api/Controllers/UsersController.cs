using GamifyMe.Shared.Constants;
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


using GamifyMe.Shared.Helpers;

namespace GamifyMe.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ObjectiveService _objectiveService;
        private readonly StoreService _storeService;
        private readonly BadgesService _badgesService;

        public UsersController(DataContext context, IConfiguration configuration, IEmailService emailService, ObjectiveService objectiveService, StoreService storeService, BadgesService badgesService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _objectiveService = objectiveService;
            _storeService = storeService;
            _badgesService = badgesService;
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


            if (establishment.MaxUsers > 0)
            {
                var currentCount = await _context.Users.CountAsync(u => u.EstablishmentId == establishment.Id);
                if (currentCount >= establishment.MaxUsers)
                {
                    return BadRequest("Le nombre maximum d'utilisateurs pour cet établissement a été atteint. Veuillez contacter l'administrateur pour augmenter votre forfait.");
                }
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
                LastActivityAt = DateTime.UtcNow,
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
                    var subject = "Confirmez votre compte MeritoPass";
                    var body = $@"
                        <h1>Bienvenue sur MeritoPass !</h1>
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
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Email confirmé.", Email = user.Email });
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
            var levelDetails = LevelHelpers.GetLevelDetails(currentXp);
            
            // Fetch fresh Establishment Name and Currency Name
            // We could rely on Claims for Name, but for CurrencyName we want it real-time if possible, 
            // or we accept it updates on login. 
            // Let's fetch it to be reactive to Admin changes immediately without re-login.
            string establishmentName = User.FindFirstValue("EstablishmentName") ?? "N/A";
            string currencyName = "Crédits";
            bool isShopEnabled = true;
            bool isGroupsEnabled = true;

            var establishmentIdClaim = User.FindFirst("EstablishmentId");
            if (establishmentIdClaim != null && Guid.TryParse(establishmentIdClaim.Value, out var establishmentId))
            {
                var est = await _context.Establishments.FindAsync(establishmentId);
                if (est != null)
                {
                    establishmentName = est.Name; // Update with fresh name too
                    currencyName = est.CurrencyName;
                    isShopEnabled = est.IsShopEnabled;
                    isGroupsEnabled = est.IsGroupsEnabled;
                }
            }
            
            return Ok(new InfoBarDto
            {
                Level = levelDetails.currentLevel,
                CurrentXp = currentXp,
                XpToNextLevel = levelDetails.xpForNextLevel,
                OtherWallets = otherWallets,
                EstablishmentName = establishmentName,
                FirstName = User.FindFirstValue("FirstName") ?? "",
                CurrencyName = currencyName,
                IsShopEnabled = isShopEnabled,
                IsGroupsEnabled = isGroupsEnabled
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

            await _storeService.CleanExpiredInventoryAsync(userId);

            var user = await _context.Users
                .Include(u => u.Establishment)
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("Utilisateur introuvable.");

            // Wallets
// ... (Top of file)
            // Wallets
            var xpWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode == "XP");
            var currencyWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode != "XP"); // Assuming 1 main currency for now

            int currentXp = (int)(xpWallet?.Balance ?? 0);
            int currentCurrency = (int)(currencyWallet?.Balance ?? 0);
            
            var levelDetails = LevelHelpers.GetLevelDetails(currentXp);

            // Rank
            int rank = await _context.Wallets
                .Where(w => w.EstablishmentId == user.EstablishmentId && w.CurrencyCode == "XP" && w.Balance > currentXp)
                .CountAsync() + 1;

            // Logs
            var validationLogs = await _context.Validations
                .Include(v => v.Objective)
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.Date)
                .Take(50)
                .ToListAsync();

            var orderLogs = await _context.Orders
                .Include(o => o.StoreItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.DatePurchased)
                .Take(50)
                .ToListAsync();

            var logs = new List<UserActivityLogDto>();
            logs.AddRange(validationLogs.Select(v => new UserActivityLogDto
            {
                Date = v.Date,
                Description = $"Objectif validé : {v.Objective.Title}",
                XpChange = v.Objective.XpReward,
                CurrencyChange = v.Objective.DocPointsReward,
                Type = "XP",
                Icon = "CheckCircle"
            }));
            logs.AddRange(orderLogs.Select(o => new UserActivityLogDto
            {
                Date = o.DatePurchased,
                Description = $"Achat : {o.StoreItem.Name}",
                XpChange = 0,
                CurrencyChange = -o.PricePaid,
                Type = "Currency",
                Icon = "ShoppingCart"
            }));

            var sortedLogs = logs.OrderByDescending(l => l.Date).Take(50).ToList();

            // Fetch Active UI Theme and QR Style
            var activeItems = await _context.UserInventories
                .Include(ui => ui.StoreItem)
                .Where(ui => ui.UserId == userId && ui.IsActive)
                .ToListAsync();

            var activeThemeItem = activeItems.FirstOrDefault(ui => ui.StoreItem.DigitalActionCode != null && ui.StoreItem.DigitalActionCode.StartsWith("UI_THEME_"));
            string activeTheme = activeThemeItem?.StoreItem.DigitalActionCode ?? GamifyMe.Shared.Constants.ThemeConstants.Default;

            var activeQrStyleItem = activeItems.FirstOrDefault(ui => ui.StoreItem.DigitalActionCode != null && ui.StoreItem.DigitalActionCode.StartsWith("QR_STYLE_"));
            string activeQrStyle = activeQrStyleItem?.StoreItem.DigitalActionCode ?? GamifyMe.Shared.Constants.ThemeConstants.QrStyleDefault;

            // Boosts
            var activeBoostItem = activeItems.FirstOrDefault(ui => ui.StoreItem.DigitalActionCode != null && ui.StoreItem.DigitalActionCode.Contains("BOOST"));
            int boostMultiplier = 1;
            DateTime? boostEndsAt = null;
            if (activeBoostItem != null)
            {
                // Simple logic: if active, it's active. Assuming expiration handles deactivation or we check it here.
                if (activeBoostItem.ExpiresAt == null || activeBoostItem.ExpiresAt > DateTime.UtcNow)
                {
                    // Parse multiplier from code if possible, or default to 2
                    if (activeBoostItem.StoreItem.DigitalActionCode.Contains("2X")) boostMultiplier = 2;
                    else if (activeBoostItem.StoreItem.DigitalActionCode.Contains("3X")) boostMultiplier = 3;
                    boostEndsAt = activeBoostItem.ExpiresAt;
                }
            }

            return Ok(new UserProfileDetailsDto
            {
                Username = user.Username,
                FirstName = user.FirstName,
                Email = user.Email,
                EstablishmentName = user.Establishment?.Name ?? "N/A",
                Role = user.Role,
                QrCode = user.QrCode,
                CreatedAt = user.CreatedAt,
                Level = levelDetails.currentLevel,
                CurrentXp = currentXp,
                XpForNextLevel = levelDetails.xpForNextLevel,
                ProgressPercentage = levelDetails.progressPercent,
                Rank = rank,
                CurrencyBalance = currentCurrency,
                CurrencyName = user.Establishment?.CurrencyName ?? "Crédits",
                IsShopEnabled = user.Establishment?.IsShopEnabled ?? true,
                IsGroupsEnabled = user.Establishment?.IsGroupsEnabled ?? true,
                GroupId = user.GroupId,
                GroupName = user.Group?.Name,
                GroupIcon = user.Group?.IconName,
                GroupColor = user.Group?.Color,
                RecentActivity = sortedLogs,
                ActiveUiTheme = activeTheme,
                ActiveQrCodeStyle = activeQrStyle,
                ActiveBoostMultiplier = boostMultiplier,
                BoostEndsAt = boostEndsAt,
                Badges = await _badgesService.GetAllBadgesAsync(userId, user.EstablishmentId)
            });
        }

        [HttpGet("inventory")]
        [Authorize]
        public async Task<ActionResult<List<UserInventoryDto>>> GetMyInventory()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            await _storeService.CleanExpiredInventoryAsync(userId);

            var inventory = await _context.UserInventories
                .Include(ui => ui.StoreItem)
                .Where(ui => ui.UserId == userId)
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
                    DigitalAssetUrl = ui.StoreItem.DigitalAssetUrl,
                    Color = ui.StoreItem.Color
                })
                .ToListAsync();

            return Ok(inventory);
        }

        [HttpPost("inventory/{userInventoryId}/toggle")]
        [Authorize]
        public async Task<IActionResult> ToggleInventoryItem(Guid userInventoryId)
        {
            var userId = GetCurrentUserId();
            var itemToToggle = await _context.UserInventories
                .Include(ui => ui.StoreItem)
                .FirstOrDefaultAsync(ui => ui.Id == userInventoryId && ui.UserId == userId);

            if (itemToToggle == null) return NotFound("Objet non trouvé.");

            // Logic:
            // If turning ON:
            // - If Theme: Turn off other themes
            // - If QR Style: Turn off other QR styles
            // - If Boost: (Maybe allow multiple? For now, let's say one boost at a time)
            // If turning OFF:
            // - Just turn off.

            if (!itemToToggle.IsActive)
            {
                // We are activating it
                string code = itemToToggle.StoreItem.DigitalActionCode;
                if (!string.IsNullOrEmpty(code))
                {
                    if (code.StartsWith("UI_THEME_"))
                    {
                        var otherThemes = await _context.UserInventories
                            .Include(ui => ui.StoreItem)
                            .Where(ui => ui.UserId == userId && ui.IsActive && ui.StoreItem.DigitalActionCode.StartsWith("UI_THEME_"))
                            .ToListAsync();
                        foreach (var item in otherThemes) item.IsActive = false;
                    }
                    else if (code.StartsWith("QR_STYLE_"))
                    {
                        var otherStyles = await _context.UserInventories
                            .Include(ui => ui.StoreItem)
                            .Where(ui => ui.UserId == userId && ui.IsActive && ui.StoreItem.DigitalActionCode.StartsWith("QR_STYLE_"))
                            .ToListAsync();
                        foreach (var item in otherStyles) item.IsActive = false;
                    }
                    else if (code == "SCAN_SOUND")
                    {
                        var otherSounds = await _context.UserInventories
                            .Include(ui => ui.StoreItem)
                            .Where(ui => ui.UserId == userId && ui.IsActive && ui.StoreItem.DigitalActionCode == "SCAN_SOUND")
                            .ToListAsync();
                        foreach (var item in otherSounds) item.IsActive = false;
                    }
                    // Add other exclusive categories here if needed
                }
                itemToToggle.IsActive = true;
            }
            else
            {
                // We are deactivating it
                itemToToggle.IsActive = false;
            }

            await _context.SaveChangesAsync();
            return Ok();
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

            // Fetch Active Objectives for this user
            var activeObjectives = await _objectiveService.GetActiveObjectivesAsync(user.Id, user.EstablishmentId);

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
                PendingOrders = pendingOrders,
                ActiveObjectives = activeObjectives
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
        [HttpPut("profile/name")]
        [Authorize]
        public async Task<ActionResult<UserProfileDetailsDto>> UpdateName(UpdateUserNameDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Utilisateur introuvable.");

            if (await _context.Users.AnyAsync(u => u.Id != userId && u.Username == request.Username))
            {
                return BadRequest("Ce nom d'utilisateur est déjà pris.");
            }

            user.FirstName = request.FirstName;
            user.Username = request.Username;

            await _context.SaveChangesAsync();
            return await GetMyProfileDetails();
        }

        [HttpPut("profile/email")]
        [Authorize]
        public async Task<IActionResult> UpdateEmail(UpdateUserEmailDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Utilisateur introuvable.");

            if (await _context.Users.AnyAsync(u => u.Id != userId && u.Email == request.NewEmail.ToLower()))
            {
                return BadRequest("Cet email est déjà utilisé.");
            }

            if (user.Email.ToLower() == request.NewEmail.ToLower())
            {
                return Ok("L'email est identique.");
            }

            // Update email and require re-confirmation
            user.Email = request.NewEmail.ToLower();
            user.IsEmailConfirmed = false;
            user.EmailConfirmationToken = Guid.NewGuid().ToString();

            await _context.SaveChangesAsync();

            // Send confirmation email
            try 
            {
                var appUrl = _configuration["AppUrl"];
                var confirmationLink = $"{appUrl}/confirm-email?token={user.EmailConfirmationToken}";
                var subject = "Confirmez votre nouvel email MeritoPass";
                var body = $@"
                    <h1>Modification d'email</h1>
                    <p>Vous avez demandé à changer votre adresse email pour votre compte MeritoPass. Veuillez cliquer sur le lien ci-dessous pour confirmer cette nouvelle adresse :</p>
                    <p><a href='{confirmationLink}'>Confirmer mon nouvel email</a></p>";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateEmail] Error sending email: {ex.Message}");
            }

            return Ok("Email mis à jour. Veuillez vérifier vos emails pour confirmer votre nouvelle adresse.");
        }

        [HttpPut("profile/password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Utilisateur introuvable.");

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            {
                return BadRequest("L'ancien mot de passe est incorrect.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok("Mot de passe modifié avec succès.");
        }
        [HttpGet("updates")]
        [Authorize]
        public async Task<ActionResult<List<UserUpdateDto>>> GetRecentUpdates([FromQuery] DateTime? since)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            // Default lookback if not provided or too old (max 5 mins to prevent massive queries if client clock is wrong)
            var threshold = since.HasValue && since.Value > DateTime.UtcNow.AddMinutes(-5) 
                ? since.Value 
                : DateTime.UtcNow.AddMinutes(-1); // Default to last 1 minute if first check or stale

            var updates = new List<UserUpdateDto>();

            // 1. Validations (Objectives)
            var validations = await _context.Validations
                .Include(v => v.Objective)
                .Where(v => v.UserId == userId && v.Date > threshold)
                .ToListAsync();

            updates.AddRange(validations.Select(v => new UserUpdateDto
            {
                Type = "Validation",
                Title = "Objectif Validé !",
                Message = $"Félicitations ! Vous avez validé l'objectif : {v.Objective.Title}",
                XpGained = v.Objective.XpReward, // Note: This doesn't account for bonuses applied at validation time, handled in client message primarily
                CurrencyGained = v.Objective.DocPointsReward,
                Date = v.Date
            }));

            // 2. Completed Orders (Store Items)
            var completedOrders = await _context.Orders
                .Include(o => o.StoreItem)
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Completed && o.DateCompleted.HasValue && o.DateCompleted.Value > threshold)
                .ToListAsync();

            updates.AddRange(completedOrders.Select(o => new UserUpdateDto
            {
                Type = "Order",
                Title = "Article Reçu !",
                Message = $"L'article '{o.StoreItem.Name}' vous a été remis.",
                XpGained = 0,
                CurrencyGained = 0,
                Date = o.DateCompleted.Value
            }));

            return Ok(updates.OrderBy(u => u.Date).ToList());
        }
        [HttpGet("{userId}/details")]
        [Authorize]
        public async Task<ActionResult<PlayerDetailsDto>> GetPlayerDetails(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Establishment)
                .Include(u => u.Group)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("Joueur introuvable.");

            var xpWallet = await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode == "XP");
            
            var currencyWallet = await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode != "XP");

            // Calculate Streaks
            // We use the service to reuse the complex streak logic
            var activeObjectives = await _objectiveService.GetActiveObjectivesAsync(userId, user.EstablishmentId);
            
            var streaks = activeObjectives
                .Where(o => o.IsStreakEnabled && o.Category == ObjectiveCategory.Principal) 
                // Determine what "Principal" means - likely checking the enum Category
                .Select(o => new PlayerObjectiveStreakDto
                {
                    ObjectiveTitle = o.Title,
                    CurrentStreak = o.CurrentStreak,
                    IconName = o.IconName
                })
                .Where(s => s.CurrentStreak > 0) // Only show active streaks? Or all? User said "meilleures séries", implies impressive ones.
                .OrderByDescending(s => s.CurrentStreak)
                .ToList();
            
            // If the user wants "Best Streaks Ever", and we only have "Current", we show Current.
            // If CurrentStreak is 0, should we show it? Maybe not. 
            // The prompt says "meilleures séries". It implies showing something positive.
            // If I have 0 streak, it's not a "best streak".

            return Ok(new PlayerDetailsDto
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.Username,
                TotalXp = (int)(xpWallet?.Balance ?? 0),
                TotalCurrency = (int)(currencyWallet?.Balance ?? 0),
                CurrencyName = user.Establishment?.CurrencyName ?? "Crédits",
                RegistrationDate = user.CreatedAt,
                GroupId = user.GroupId,
                GroupName = user.Group?.Name,
                GroupIcon = user.Group?.IconName,
                GroupColor = user.Group?.Color,
                GroupImageUrl = user.Group?.ImageUrl,
                PrincipalStreaks = streaks,
                Badges = await _badgesService.GetAllBadgesAsync(userId, user.EstablishmentId)
            });
        }


        [HttpGet("establishment/users")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<ActionResult<List<UserSummaryDto>>> GetEstablishmentUsers()
        {
            var currentUserId = GetCurrentUserId();
            var establishmentIdClaim = User.FindFirst("EstablishmentId");
            if (establishmentIdClaim == null || !Guid.TryParse(establishmentIdClaim.Value, out var establishmentId))
            {
                return Unauthorized();
            }

            // Using projection for performance
            var users = await _context.Users
                .Where(u => u.EstablishmentId == establishmentId && u.Id != currentUserId) // Optionally exclude self? Or keep self. Let's keep self but maybe highlight it in UI.
                .Select(u => new UserSummaryDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.Username, // Mapping logic from profile endpoint seems to use Username as LastName? Or just use Username.
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    LastActivityAt = u.LastActivityAt,
                    Status = u.Status,
                    XpBalance = (int)(u.Wallets.Where(w => w.CurrencyCode == "XP").Select(w => w.Balance).FirstOrDefault()),
                    CurrencyBalance = (int)(u.Wallets.Where(w => w.CurrencyCode != "XP").Select(w => w.Balance).FirstOrDefault())
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("establishment/create-user")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<ActionResult> CreateUser(CreateUserDto request)
        {
            var establishmentIdClaim = User.FindFirst("EstablishmentId");
            if (establishmentIdClaim == null || !Guid.TryParse(establishmentIdClaim.Value, out var establishmentId))
            {
                return Unauthorized();
            }

            
            var establishment = await _context.Establishments.FindAsync(establishmentId);
            if (establishment != null && establishment.MaxUsers > 0)
            {
                var currentCount = await _context.Users.CountAsync(u => u.EstablishmentId == establishmentId);
                if (currentCount >= establishment.MaxUsers)
                {
                     return BadRequest("Le nombre maximum d'utilisateurs a été atteint.");
                }
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email.ToLower()))
            {
                return BadRequest("Cet email est déjà utilisé.");
            }
             if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Ce nom d'utilisateur est déjà utilisé.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                FirstName = request.FirstName,
                Email = request.Email.ToLower(),
                PasswordHash = passwordHash,
                Role = request.Role,
                EstablishmentId = establishmentId,
                QrCode = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsEmailConfirmed = true, // Admin created users are auto-confirmed
                EmailConfirmationToken = null
            };

            var xpWallet = new Wallet { Id = Guid.NewGuid(), EstablishmentId = establishmentId, UserId = user.Id, CurrencyCode = "XP", Balance = 0 };
            var currencyWallet = new Wallet { Id = Guid.NewGuid(), EstablishmentId = establishmentId, UserId = user.Id, CurrencyCode = "DOC", Balance = 0 };

            _context.Users.Add(user);
            _context.Wallets.Add(xpWallet);
            _context.Wallets.Add(currencyWallet);

            await _context.SaveChangesAsync();

            return Ok("Utilisateur créé avec succès.");
        }

        [HttpPut("establishment/users/{userId}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
        public async Task<IActionResult> UpdateUserAsAdmin(Guid userId, UpdateUserAdminDto request)
        {
            var establishmentIdClaim = User.FindFirst("EstablishmentId");
            if (establishmentIdClaim == null || !Guid.TryParse(establishmentIdClaim.Value, out var establishmentId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.EstablishmentId == establishmentId);
            if (user == null)
            {
                return NotFound("Utilisateur introuvable.");
            }

            // Uniqueness check for username/email excluding current user
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
            user.Role = request.Role;

            user.Role = request.Role;

            // Update Wallets
            var wallets = await _context.Wallets.Where(w => w.UserId == userId).ToListAsync();
            var xpWallet = wallets.FirstOrDefault(w => w.CurrencyCode == "XP");
            var currencyWallet = wallets.FirstOrDefault(w => w.CurrencyCode != "XP");

            if (xpWallet != null) xpWallet.Balance = request.XpBalance;
            if (currencyWallet != null) currencyWallet.Balance = request.CurrencyBalance;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Utilisateur mis à jour avec succès." });
        }

        [HttpDelete("establishment/users/{userId}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var establishmentIdClaim = User.FindFirst("EstablishmentId");
            if (establishmentIdClaim == null || !Guid.TryParse(establishmentIdClaim.Value, out var establishmentId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.EstablishmentId == establishmentId);
            if (user == null)
            {
                return NotFound("Utilisateur introuvable ou n'appartient pas à cet établissement.");
            }

            // Optional: Check if trying to delete self?
            var currentUserId = GetCurrentUserId();
            if (user.Id == currentUserId)
            {
                 return BadRequest("Vous ne pouvez pas supprimer votre propre compte ici.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}