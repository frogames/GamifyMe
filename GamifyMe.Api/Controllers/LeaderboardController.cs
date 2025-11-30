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
    public class LeaderboardController : ControllerBase
    {
        private readonly DataContext _context;

        public LeaderboardController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("users-xp")]
        public async Task<ActionResult<List<UserLeaderboardEntryDto>>> GetUserXpLeaderboard()
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);

            var users = await _context.Users
                .Where(u => u.EstablishmentId == establishmentId) 
                .Include(u => u.Wallets)
                .Include(u => u.Group)
                .ToListAsync();

            var leaderboard = users
                .Select(u => new 
                {
                    User = u,
                    Xp = (int)(u.Wallets.FirstOrDefault(w => w.CurrencyCode == "XP")?.Balance ?? 0),
                    Currency = (int)(u.Wallets.FirstOrDefault(w => w.CurrencyCode != "XP")?.Balance ?? 0)
                })
                .OrderByDescending(x => x.Xp)
                .Select((x, index) => new UserLeaderboardEntryDto
                {
                    Rank = index + 1,
                    UserId = x.User.Id,
                    Username = x.User.Username,
                    FirstName = x.User.FirstName,
                    Level = 1 + (x.Xp / 500),
                    TotalXp = x.Xp,
                    TotalCurrency = x.Currency,
                    GroupName = x.User.Group?.Name
                })
                .Take(50)
                .ToList();

            return Ok(leaderboard);
        }

        [HttpGet("users-currency")]
        public async Task<ActionResult<List<UserLeaderboardEntryDto>>> GetUserCurrencyLeaderboard()
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);

            var users = await _context.Users
                .Where(u => u.EstablishmentId == establishmentId) 
                .Include(u => u.Wallets)
                .Include(u => u.Group)
                .ToListAsync();

            var leaderboard = users
                .Select(u => new 
                {
                    User = u,
                    Xp = (int)(u.Wallets.FirstOrDefault(w => w.CurrencyCode == "XP")?.Balance ?? 0),
                    Currency = (int)(u.Wallets.FirstOrDefault(w => w.CurrencyCode != "XP")?.Balance ?? 0)
                })
                .OrderByDescending(x => x.Currency)
                .Select((x, index) => new UserLeaderboardEntryDto
                {
                    Rank = index + 1,
                    UserId = x.User.Id,
                    Username = x.User.Username,
                    FirstName = x.User.FirstName,
                    Level = 1 + (x.Xp / 500),
                    TotalXp = x.Xp,
                    TotalCurrency = x.Currency,
                    GroupName = x.User.Group?.Name
                })
                .Take(50)
                .ToList();

            return Ok(leaderboard);
        }

        [HttpGet("groups-xp")]
        public async Task<ActionResult<List<GroupLeaderboardEntryDto>>> GetGroupXpLeaderboard()
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);

            var groups = await _context.Groups
                .Where(g => g.EstablishmentId == establishmentId)
                .OrderByDescending(g => g.TotalXp)
                .Include(g => g.Members)
                .Take(20)
                .ToListAsync();

            var leaderboard = groups.Select((g, index) => new GroupLeaderboardEntryDto
            {
                Rank = index + 1,
                GroupId = g.Id,
                Name = g.Name,
                IconName = g.IconName,
                MemberCount = g.Members.Count,
                TotalXp = g.TotalXp
            }).ToList();

            return Ok(leaderboard);
        }

        [HttpGet("groups-members")]
        public async Task<ActionResult<List<GroupLeaderboardEntryDto>>> GetGroupMembersLeaderboard()
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);

            var groups = await _context.Groups
                .Where(g => g.EstablishmentId == establishmentId)
                .Include(g => g.Members)
                .OrderByDescending(g => g.Members.Count)
                .Take(20)
                .ToListAsync();

            var leaderboard = groups.Select((g, index) => new GroupLeaderboardEntryDto
            {
                Rank = index + 1,
                GroupId = g.Id,
                Name = g.Name,
                IconName = g.IconName,
                MemberCount = g.Members.Count,
                TotalXp = g.TotalXp
            }).ToList();

            return Ok(leaderboard);
        }
    }
}
