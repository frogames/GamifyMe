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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly DataContext _dbContext;

        private readonly CurrencyService _currencyService;

        public StoreController(DataContext context, CurrencyService currencyService)
        {
            _dbContext = context;
            _currencyService = currencyService;
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<StoreItemDto>>> GetActiveStoreItems()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var items = await _dbContext.StoreItems
                .Where(item => item.IsActive && item.Stock > 0)
                .OrderBy(item => item.Price)
                .ToListAsync();

            // Fetch user's inventory/orders to determine ownership
            var userInventoryIds = await _dbContext.UserInventories
                .Where(ui => ui.UserId == userId)
                .Select(ui => ui.StoreItemId)
                .ToListAsync();
            
            var userOrderIds = await _dbContext.Orders
                .Where(o => o.UserId == userId && o.Status != OrderStatus.Cancelled)
                .Select(o => o.StoreItemId)
                .ToListAsync();

            var ownedItemIds = userInventoryIds.Union(userOrderIds).Distinct().ToHashSet();

            var dtos = items.Select(item => new StoreItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                IconName = item.IconName,
                Price = item.Price,
                Stock = item.Stock,
                ItemType = item.ItemType,
                DigitalActionCode = item.DigitalActionCode,
                DigitalAssetUrl = item.DigitalAssetUrl,
                IsUnique = item.IsUnique,
                IsOwned = ownedItemIds.Contains(item.Id)
            }).ToList();

            return Ok(dtos);
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> CreateStoreItem(StoreItemDto request)
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var storeItem = new StoreItem
            {
                Id = Guid.NewGuid(),
                EstablishmentId = establishmentId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                ItemType = request.ItemType,
                DigitalActionCode = request.DigitalActionCode,
                DigitalAssetUrl = request.DigitalAssetUrl,
                IconName = request.IconName ?? "ShoppingBag",
                IsActive = request.IsActive,
                IsUnique = request.IsUnique,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            _dbContext.StoreItems.Add(storeItem);
            await _dbContext.SaveChangesAsync();

            var resultDto = new StoreItemDto { Id = storeItem.Id /* Mappez le reste */ };
            return CreatedAtAction(nameof(GetStoreItemById), new { id = resultDto.Id }, resultDto);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur},{Roles.Gestionnaire}")]
        public async Task<ActionResult<StoreItemDto>> GetStoreItemById(Guid id)
        {
            var storeItem = await _dbContext.StoreItems.FindAsync(id);
            if (storeItem == null) return NotFound();

            // Map to DTO
            var dto = new StoreItemDto
            {
                Id = storeItem.Id,
                Name = storeItem.Name,
                Description = storeItem.Description,
                ItemType = storeItem.ItemType,
                Price = storeItem.Price,
                Stock = storeItem.Stock,
                IconName = storeItem.IconName,
                IsActive = storeItem.IsActive,
                DigitalActionCode = storeItem.DigitalActionCode,
                DigitalAssetUrl = storeItem.DigitalAssetUrl
            };
            return Ok(dto);
        }

        [HttpGet("list-all")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur},{Roles.Gestionnaire}")]
        public async Task<ActionResult<List<StoreItemDto>>> GetAllStoreItemsSimpleList()
        {
            var items = await _dbContext.StoreItems
                .OrderBy(item => item.Name)
                .Select(item => new StoreItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    ItemType = item.ItemType,
                    Price = item.Price,
                    Stock = item.Stock,
                    IconName = item.IconName,
                    IsActive = item.IsActive,
                    DigitalActionCode = item.DigitalActionCode,
                    DigitalAssetUrl = item.DigitalAssetUrl
                })
                .ToListAsync();
            return Ok(items);
        }

        [HttpPost("purchase/{itemId}")]
        [Authorize]
        public async Task<IActionResult> PurchaseStoreItem(Guid itemId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var storeItem = await _dbContext.StoreItems.FindAsync(itemId);
                if (storeItem == null || !storeItem.IsActive) return NotFound("Objet non disponible.");
                if (storeItem.Stock <= 0) return BadRequest("Stock épuisé.");

                var userWallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode != "XP");
                if (userWallet == null)
                {
                    var establishmentIdClaim = User.FindFirstValue("EstablishmentId");
                    if (string.IsNullOrEmpty(establishmentIdClaim)) return BadRequest("Impossible de récupérer l'établissement de l'utilisateur.");

                    userWallet = new Wallet
                    {
                        Id = Guid.NewGuid(),
                        EstablishmentId = Guid.Parse(establishmentIdClaim),
                        UserId = userId,
                        CurrencyCode = "DOC",
                        Balance = 0
                    };
                    _dbContext.Wallets.Add(userWallet);
                    await _dbContext.SaveChangesAsync();
                }
                if (userWallet.Balance < storeItem.Price) return BadRequest("Fonds insuffisants.");

                userWallet.Balance -= storeItem.Price;
                storeItem.Stock -= 1;

                var orderStatus = OrderStatus.Pending;

                // AJOUT : On ajoute TOUS les objets à l'inventaire (Physique ou Digital)
                var inventoryItem = new UserInventory
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    StoreItemId = storeItem.Id,
                    EstablishmentId = storeItem.EstablishmentId,
                    DateAcquired = DateTime.UtcNow,
                    IsActive = false // Par défaut inactif
                };
                _dbContext.UserInventories.Add(inventoryItem);

                if (storeItem.ItemType == StoreItemType.Digital)
                {
                    orderStatus = OrderStatus.Completed;
                }

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    StoreItemId = storeItem.Id,
                    EstablishmentId = storeItem.EstablishmentId,
                    PricePaid = storeItem.Price,
                    DatePurchased = DateTime.UtcNow,
                    Status = orderStatus
                };
                _dbContext.Orders.Add(order);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Achat réussi !");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Erreur d'achat : {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> UpdateStoreItem(Guid id, StoreItemDto request)
        {
            var storeItem = await _dbContext.StoreItems.FindAsync(id);
            if (storeItem == null) return NotFound("Objet non trouvé.");

            storeItem.Name = request.Name;
            storeItem.Description = request.Description;
            storeItem.Price = request.Price;
            storeItem.Stock = request.Stock;
            storeItem.ItemType = request.ItemType;
            storeItem.DigitalActionCode = request.DigitalActionCode;
            storeItem.DigitalAssetUrl = request.DigitalAssetUrl;
            storeItem.IconName = request.IconName ?? "ShoppingBag";
            storeItem.IsActive = request.IsActive;
            storeItem.IsUnique = request.IsUnique;
            storeItem.StartDate = request.StartDate;
            storeItem.EndDate = request.EndDate;

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> DeleteStoreItem(Guid id)
        {
            var storeItem = await _dbContext.StoreItems.FindAsync(id);
            if (storeItem == null) return NotFound("Objet non trouvé.");

            _dbContext.StoreItems.Remove(storeItem);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}