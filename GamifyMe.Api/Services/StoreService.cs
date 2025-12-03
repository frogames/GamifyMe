using GamifyMe.Api.Data;
using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GamifyMe.Api.Services
{
    public class StoreService
    {
        private readonly DataContext _dbContext;

        public StoreService(DataContext context)
        {
            _dbContext = context;
        }

        public async Task<List<StoreItemDto>> GetActiveStoreItemsAsync(Guid userId)
        {
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

            return items.Select(item => new StoreItemDto
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
                IsOwned = ownedItemIds.Contains(item.Id),
                ImageUrl = item.ImageUrl,
                Color = item.Color
            }).ToList();
        }

        public async Task<List<StoreItemDto>> GetAllStoreItemsSimpleListAsync()
        {
            return await _dbContext.StoreItems
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
                    DigitalAssetUrl = item.DigitalAssetUrl,
                    ImageUrl = item.ImageUrl,
                    Color = item.Color
                })
                .ToListAsync();
        }

        public async Task<StoreItemDto?> GetStoreItemByIdAsync(Guid id)
        {
            var storeItem = await _dbContext.StoreItems.FindAsync(id);
            if (storeItem == null) return null;

            return new StoreItemDto
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
                DigitalAssetUrl = storeItem.DigitalAssetUrl,
                ImageUrl = storeItem.ImageUrl,
                Color = storeItem.Color
            };
        }

        public async Task<StoreItemDto> CreateStoreItemAsync(StoreItemDto request, Guid establishmentId)
        {
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
                IconName = GetAutoIconName(request) ?? request.IconName ?? "ShoppingBag",
                IsActive = request.IsActive,
                IsUnique = request.IsUnique,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ImageUrl = request.ImageUrl,
                Color = request.Color
            };

            _dbContext.StoreItems.Add(storeItem);
            await _dbContext.SaveChangesAsync();

            request.Id = storeItem.Id;
            return request;
        }

        public async Task<bool> UpdateStoreItemAsync(Guid id, StoreItemDto request)
        {
            var storeItem = await _dbContext.StoreItems.FindAsync(id);
            if (storeItem == null) return false;

            storeItem.Name = request.Name;
            storeItem.Description = request.Description;
            storeItem.Price = request.Price;
            storeItem.Stock = request.Stock;
            storeItem.ItemType = request.ItemType;
            storeItem.DigitalActionCode = request.DigitalActionCode;
            storeItem.DigitalAssetUrl = request.DigitalAssetUrl;
            storeItem.IconName = GetAutoIconName(request) ?? request.IconName ?? "ShoppingBag";
            storeItem.IsActive = request.IsActive;
            storeItem.IsUnique = request.IsUnique;
            storeItem.StartDate = request.StartDate;
            storeItem.EndDate = request.EndDate;
            storeItem.ImageUrl = request.ImageUrl;
            storeItem.Color = request.Color;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        private string? GetAutoIconName(StoreItemDto request)
        {
            if (request.ItemType == StoreItemType.Digital && !string.IsNullOrEmpty(request.DigitalActionCode))
            {
                if (request.DigitalActionCode == "SCAN_SOUND") return "Music";
                if (request.DigitalActionCode.StartsWith("UI_THEME_")) return "Phone";
                if (request.DigitalActionCode.Contains("BOOST")) return "Rocket";
                if (request.DigitalActionCode.StartsWith("QR_STYLE_")) return "QrCode";
            }
            return null;
        }

        public async Task<bool> DeleteStoreItemAsync(Guid id)
        {
            var storeItem = await _dbContext.StoreItems.FindAsync(id);
            if (storeItem == null) return false;

            _dbContext.StoreItems.Remove(storeItem);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message)> PurchaseStoreItemAsync(Guid itemId, Guid userId, Guid? establishmentId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var storeItem = await _dbContext.StoreItems.FindAsync(itemId);
                if (storeItem == null || !storeItem.IsActive) return (false, "Objet non disponible.");
                if (storeItem.Stock <= 0) return (false, "Stock épuisé.");

                if (storeItem.IsUnique)
                {
                    var alreadyOwned = await _dbContext.UserInventories
                        .AnyAsync(ui => ui.UserId == userId && ui.StoreItemId == itemId);
                    
                    if (alreadyOwned) return (false, "Vous possédez déjà cet objet unique.");
                }

                var userWallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode != "XP");
                if (userWallet == null)
                {
                    if (establishmentId == null) return (false, "Impossible de récupérer l'établissement de l'utilisateur.");

                    userWallet = new Wallet
                    {
                        Id = Guid.NewGuid(),
                        EstablishmentId = establishmentId.Value,
                        UserId = userId,
                        CurrencyCode = "DOC",
                        Balance = 0
                    };
                    _dbContext.Wallets.Add(userWallet);
                    await _dbContext.SaveChangesAsync();
                }

                if (userWallet.Balance < storeItem.Price) return (false, "Fonds insuffisants.");

                userWallet.Balance -= storeItem.Price;
                
                // Only decrement stock for physical items
                if (storeItem.ItemType != StoreItemType.Digital)
                {
                    storeItem.Stock -= 1;
                }

                var orderStatus = OrderStatus.Pending;

                // Boost Logic
                bool isBoost = storeItem.DigitalActionCode != null && storeItem.DigitalActionCode.Contains("BOOST", StringComparison.OrdinalIgnoreCase);
                DateTime? expiresAt = null;
                bool isActive = false;

                if (isBoost)
                {
                    // Check if user already has an active boost
                    var hasActiveBoost = await _dbContext.UserInventories
                        .Include(ui => ui.StoreItem)
                        .AnyAsync(ui => ui.UserId == userId && 
                                        ui.IsActive && 
                                        ui.StoreItem.DigitalActionCode != null && 
                                        ui.StoreItem.DigitalActionCode.Contains("BOOST") &&
                                        (ui.ExpiresAt == null || ui.ExpiresAt > DateTime.UtcNow));

                    if (hasActiveBoost)
                    {
                        await transaction.RollbackAsync();
                        return (false, "Vous avez déjà un boost actif. Attendez qu'il expire.");
                    }

                    isActive = true;
                    expiresAt = DateTime.UtcNow.AddHours(24);
                }

                // Add to inventory
                var inventoryItem = new UserInventory
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    StoreItemId = storeItem.Id,
                    EstablishmentId = storeItem.EstablishmentId,
                    DateAcquired = DateTime.UtcNow,
                    IsActive = isActive,
                    ExpiresAt = expiresAt
                };
                _dbContext.UserInventories.Add(inventoryItem);

                Console.WriteLine($"[Purchase] Item: {storeItem.Name}, Type: {storeItem.ItemType} ({(int)storeItem.ItemType})");

                if (storeItem.ItemType == StoreItemType.Digital)
                {
                    Console.WriteLine("[Purchase] Auto-completing Digital Order");
                    orderStatus = OrderStatus.Completed;
                }
                else
                {
                    Console.WriteLine("[Purchase] Creating Pending Order for Physical Item");
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

                return (true, "Achat réussi !");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Erreur d'achat : {ex.Message}");
            }
        }
    }
}
