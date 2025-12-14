
using GamifyMe.Api.Data;
using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Constants;
using GamifyMe.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Globalization;

namespace GamifyMe.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur},{Roles.Gestionnaire}")]
    public class StatsController : ControllerBase
    {
        private readonly DataContext _context;

        public StatsController(DataContext context)
        {
            _context = context;
        }

        private Guid GetCurrentEstablishmentId()
        {
            var claim = User.FindFirst("EstablishmentId");
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
        }

        [HttpGet("detailed")]
        public async Task<ActionResult<AdminDetailedStatsDto>> GetDetailedStats()
        {
            var establishmentId = GetCurrentEstablishmentId();
            if (establishmentId == Guid.Empty) return Unauthorized();

            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);

            // 1. Key Figures
            var totalUsers = await _context.Users.CountAsync(u => u.EstablishmentId == establishmentId);
            var newUsers30d = await _context.Users.CountAsync(u => u.EstablishmentId == establishmentId && u.CreatedAt >= thirtyDaysAgo);
            var activeUsers30d = await _context.Users.CountAsync(u => u.EstablishmentId == establishmentId && u.LastActivityAt >= thirtyDaysAgo);
            
            // For Total XP, looking at Validation logs is safer than Wallets because Wallets balance fluctuates (spending).
            // "Total XP Distributed" typically means XP earned.
            // If using Validations:
            var totalXpDistributed = await _context.Validations
                .Include(v => v.Objective)
                .Where(v => v.EstablishmentId == establishmentId)
                .SumAsync(v => (long)v.Objective.XpReward); // Casting to long to avoid overflow

            var totalValidations = await _context.Validations.CountAsync(v => v.EstablishmentId == establishmentId);

            // 2. User Growth (Last 6 Months)
            var sixMonthsAgo = now.AddMonths(-5); // Including current month -> total 6
            var startDate = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1); // Start of that month

            // EF Core GroupBy with Date can be tricky with providers. 
            // Better to fetch meaningful data and process in memory if dataset is not huge, 
            // OR use raw SQL / specialized functions.
            // For User Growth, fetching CreateDate of all users in establishment isn't too heavy usually (assuming < 10k users).
            // Let's try in-memory grouping for now to avoid extensive LINQ translation issues with Npgsql.
            
            var usersDates = await _context.Users
                .Where(u => u.EstablishmentId == establishmentId && u.CreatedAt >= startDate)
                .Select(u => u.CreatedAt)
                .ToListAsync();

            var growthData = new List<StatsUserGrowthDto>();
            // Also need cumulative count BEFORE start date
            var initialCount = await _context.Users.CountAsync(u => u.EstablishmentId == establishmentId && u.CreatedAt < startDate);
            
            var currentCumulative = initialCount;
            
            for (int i = 0; i < 6; i++)
            {
                var targetMonth = startDate.AddMonths(i);
                var nextMonth = targetMonth.AddMonths(1);
                
                var countInMonth = usersDates.Count(d => d >= targetMonth && d < nextMonth);
                currentCumulative += countInMonth;

                growthData.Add(new StatsUserGrowthDto
                {
                    Period = targetMonth.ToString("MMM yy", CultureInfo.CurrentCulture),
                    NewUsers = countInMonth,
                    CumulativeUsers = currentCumulative
                });
            }

            // 3. Objective KPIs
            // We want completion rate per objective.
            // Active objectives only? Or all? Let's take Active objectives to keep list relevant.
            
            var objectives = await _context.Objectives
                .Where(o => o.EstablishmentId == establishmentId && o.IsActive)
                .Select(o => new { o.Id, o.Title })
                .ToListAsync();

            var validations = await _context.Validations
                .Where(v => v.EstablishmentId == establishmentId && v.Date >= startDate) // Last 6 months for relevance? Or all time? User didn't specify. All time relevant for "Completion Rate".
                .Select(v => new { v.ObjectiveId, v.UserId })
                .ToListAsync();

            var kpis = new List<StatsObjectiveKpiDto>();
            
            // Avoid div by zero
            double totalUserBase = totalUsers > 0 ? (double)totalUsers : 1.0;

            foreach (var obj in objectives)
            {
                var objValidations = validations.Where(v => v.ObjectiveId == obj.Id).ToList();
                var uniqueUsers = objValidations.Select(v => v.UserId).Distinct().Count();
                
                kpis.Add(new StatsObjectiveKpiDto
                {
                    ObjectiveTitle = obj.Title,
                    ValidationCount = objValidations.Count,
                    UniqueUsers = uniqueUsers,
                    CompletionRate = Math.Round((double)uniqueUsers / totalUserBase * 100, 1) // Percentage
                });
            }

            return Ok(new AdminDetailedStatsDto
            {
                KeyFigures = new StatsKeyFiguresDto
                {
                    TotalUsers = totalUsers,
                    NewUsersLast30Days = newUsers30d,
                    ActiveUsersLast30Days = activeUsers30d,
                    TotalXpDistributed = totalXpDistributed,
                    TotalValidations = totalValidations
                },
                UserGrowth = growthData,
                ObjectiveKpis = kpis.OrderByDescending(k => k.CompletionRate).ToList()
            });
        }

        [HttpGet("growth")]
        public async Task<ActionResult<List<StatsUserGrowthDto>>> GetGrowthStats([FromQuery] string period = "month")
        {
            var establishmentId = GetCurrentEstablishmentId();
            if (establishmentId == Guid.Empty) return Unauthorized();

            var now = DateTime.UtcNow;
            DateTime startDate;
            int intervals;
            Func<DateTime, int, DateTime> addInterval;
            Func<DateTime, string> formatLabel;

            switch (period.ToLower())
            {
                case "week":
                    intervals = 12;
                    // Start of current week (Monday) - 11 weeks
                    var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                    var monday = now.AddDays(-1 * diff).Date;
                    startDate = monday.AddDays(-7 * (intervals - 1));
                    addInterval = (d, i) => d.AddDays(7 * i);
                    formatLabel = d => $"Sem {CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(d, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)}";
                    break;
                case "year":
                    intervals = 5;
                    startDate = new DateTime(now.Year - (intervals - 1), 1, 1);
                    addInterval = (d, i) => d.AddYears(i);
                    formatLabel = d => d.ToString("yyyy");
                    break;
                case "month":
                default:
                    intervals = 6;
                    startDate = new DateTime(now.AddMonths(-(intervals - 1)).Year, now.AddMonths(-(intervals - 1)).Month, 1);
                    addInterval = (d, i) => d.AddMonths(i);
                    formatLabel = d => d.ToString("MMM yy", CultureInfo.CurrentCulture);
                    break;
            }

            var usersDates = await _context.Users
                .Where(u => u.EstablishmentId == establishmentId && u.CreatedAt >= startDate)
                .Select(u => u.CreatedAt)
                .ToListAsync();

            var initialCount = await _context.Users.CountAsync(u => u.EstablishmentId == establishmentId && u.CreatedAt < startDate);
            var currentCumulative = initialCount;
            var growthData = new List<StatsUserGrowthDto>();

            for (int i = 0; i < intervals; i++)
            {
                var currentStart = addInterval(startDate, i);
                var currentEnd = addInterval(startDate, i + 1);
                
                var count = usersDates.Count(d => d >= currentStart && d < currentEnd);
                currentCumulative += count;

                growthData.Add(new StatsUserGrowthDto
                {
                    Period = formatLabel(currentStart),
                    NewUsers = count,
                    CumulativeUsers = currentCumulative
                });
            }

            return Ok(growthData);
        }
    }
}
