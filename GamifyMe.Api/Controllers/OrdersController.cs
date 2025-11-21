using GamifyMe.Api.Data;
using GamifyMe.Shared.Models;
using GamifyMe.Api.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamifyMe.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur},{Roles.Gestionnaire}")]
    public class OrdersController : ControllerBase
    {
        private readonly DataContext _context;

        public OrdersController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("{orderId}/complete")]
        public async Task<IActionResult> CompleteOrder(Guid orderId)
        {
            var order = await _context.Orders.Include(o => o.StoreItem).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound("Commande introuvable.");

            if (order.Status != OrderStatus.Pending)
                return BadRequest("Cette commande n'est pas en attente.");

            order.Status = OrderStatus.Completed;
            order.DateCompleted = DateTime.UtcNow;

            // Activate the item in user's inventory
            var inventoryItem = await _context.UserInventories
                .FirstOrDefaultAsync(ui => ui.UserId == order.UserId && ui.StoreItemId == order.StoreItemId && !ui.IsActive);

            if (inventoryItem != null)
            {
                inventoryItem.IsActive = true;
                inventoryItem.DateAcquired = DateTime.UtcNow; // Update acquired date to when it was actually received? Or keep purchase date?
                // Let's keep purchase date as acquired date, but mark as active.
            }

            await _context.SaveChangesAsync();
            return Ok("Commande validée.");
        }

        [HttpPost("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound("Commande introuvable.");

            if (order.Status != OrderStatus.Pending)
                return BadRequest("Cette commande n'est pas en attente.");

            order.Status = OrderStatus.Cancelled;

            // Refund the user
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == order.UserId && w.CurrencyCode == "DOC");
            if (wallet != null)
            {
                wallet.Balance += order.PricePaid;
            }

            // Remove from inventory
            var inventoryItem = await _context.UserInventories
                .FirstOrDefaultAsync(ui => ui.UserId == order.UserId && ui.StoreItemId == order.StoreItemId && !ui.IsActive);

            if (inventoryItem != null)
            {
                _context.UserInventories.Remove(inventoryItem);
            }

            await _context.SaveChangesAsync();
            return Ok("Commande annulée et remboursée.");
        }
    }
}
