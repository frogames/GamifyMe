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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly DataContext _context;

        public StoreController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<StoreItemDto>>> GetActiveStoreItems()
        {
            var items = await _context.StoreItems
                .Where(item => item.IsActive && item.Stock > 0)
                .OrderBy(item => item.Price)
                .Select(item => new StoreItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    IconName = item.IconName,
                    Price = item.Price,
                    Stock = item.Stock,
                    ItemType = item.ItemType,
                    DigitalActionCode = null // Ne pas montrer le code
                })
                .ToListAsync();
            return Ok(items);
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
                IconName = request.IconName ?? "ShoppingBag",
                IsActive = request.IsActive
            };

            _context.StoreItems.Add(storeItem);
            await _context.SaveChangesAsync();

            var resultDto = new StoreItemDto { Id = storeItem.Id /* Mappez le reste */ };
            return CreatedAtAction(nameof(GetStoreItemById), new { id = resultDto.Id }, resultDto);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur},{Roles.Gestionnaire}")]
        public async Task<ActionResult<StoreItemDto>> GetStoreItemById(Guid id)
        {
            var storeItem = await _context.StoreItems.FindAsync(id);
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
                DigitalActionCode = storeItem.DigitalActionCode
            };
            return Ok(dto);
        }

        [HttpGet("list-all")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur},{Roles.Gestionnaire}")]
        public async Task<ActionResult<List<StoreItemDto>>> GetAllStoreItemsSimpleList()
        {
            var items = await _context.StoreItems
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
                    DigitalActionCode = item.DigitalActionCode
                })
                .ToListAsync();
            return Ok(items);
        }

        [HttpPost("purchase/{itemId}")]
        [Authorize]
        public async Task<IActionResult> PurchaseStoreItem(Guid itemId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var storeItem = await _context.StoreItems.FindAsync(itemId);
                if (storeItem == null || !storeItem.IsActive) return NotFound("Objet non disponible.");
                if (storeItem.Stock <= 0) return BadRequest("Stock épuisé.");

                var userWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode != "XP");
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
                    _context.Wallets.Add(userWallet);
                    await _context.SaveChangesAsync();
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
                _context.UserInventories.Add(inventoryItem);

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
                _context.Orders.Add(order);

                await _context.SaveChangesAsync();
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
            var storeItem = await _context.StoreItems.FindAsync(id);
            if (storeItem == null) return NotFound("Objet non trouvé.");

            storeItem.Name = request.Name;
            storeItem.Description = request.Description;
            storeItem.Price = request.Price;
            storeItem.Stock = request.Stock;
            storeItem.ItemType = request.ItemType;
            storeItem.DigitalActionCode = request.DigitalActionCode;
            storeItem.IconName = request.IconName ?? "ShoppingBag";
            storeItem.IsActive = request.IsActive;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur}")]
        public async Task<IActionResult> DeleteStoreItem(Guid id)
        {
            var storeItem = await _context.StoreItems.FindAsync(id);
            if (storeItem == null) return NotFound("Objet non trouvé.");

            _context.StoreItems.Remove(storeItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}