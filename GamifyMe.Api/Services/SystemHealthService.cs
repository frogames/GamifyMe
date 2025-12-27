using GamifyMe.Api.Data;
using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GamifyMe.Api.Services
{
    public class SystemHealthService
    {
        private readonly DataContext _context;
        private readonly StrategyConfigurationService _strategyService;

        public SystemHealthService(DataContext context, StrategyConfigurationService strategyService)
        {
            _context = context;
            _strategyService = strategyService;
        }

        public async Task<SystemHealthDto> CalculateHealthAsync(Guid establishmentId)
        {
            var today = DateTime.UtcNow;
            var thirtyDaysAgo = today.AddDays(-30);

            // 1. Get Contextual Data
            var strategy = await _strategyService.GetStrategyAsync(establishmentId);
            var users = await _context.Users
                .Where(u => u.EstablishmentId == establishmentId && u.Status == "active")
                .Include(u => u.Wallets)
                .ToListAsync();

            var validations = await _context.Validations
                .Where(v => v.EstablishmentId == establishmentId && v.Date >= thirtyDaysAgo)
                .Include(v => v.Objective)
                .ToListAsync();

            var orders = await _context.Orders
                .Where(o => o.EstablishmentId == establishmentId && o.DatePurchased >= thirtyDaysAgo)
                .ToListAsync();

            var totalUsers = users.Count;
            if (totalUsers == 0) return new SystemHealthDto { Advice = new List<string> { "Pas assez d'utilisateurs pour l'analyse." } };

            // 2. Calculate Indicators

            // --- A. ECONOMY (Inflation / Hoarding) ---
            double moneyCreated = validations.Sum(v => v.Objective?.DocPointsReward ?? 0); 
            double moneyDestroyed = orders.Sum(o => o.PricePaid);
            
            // Avoid division by zero
            double inflationRate = moneyDestroyed > 0 ? moneyCreated / moneyDestroyed : (moneyCreated > 0 ? 10.0 : 0); // >1 means inflation

            double totalWallet = users.Sum(u => u.CurrentXp); // This is XP, wait. Wallet is separate.
            // Using in-memory sum for wallets, User has List<Wallet>. Assuming 1 main currency for now or sum all.
            // Establishment.CurrencyName helps but typically we just sum amounts.
            double totalMoneyHoarded = users.SelectMany(u => u.Wallets).Sum(w => w.Balance);
            
            // Proxy for Total Generated Ever is hard without full log history sum, 
            // but Hoarding Rate = Balance / (Balance + Spent_Lifetime). 
            // Let's approximate Hoarding Rate = Balance / (Balance + Orders_Lifetime_Sum) if possible, 
            // or just use arbitrary "Average Per User" threshold.
            // For now, let's use a "Velocity" metrics.
            double hoardingRate = totalMoneyHoarded > 0 ? (totalMoneyHoarded / (totalMoneyHoarded + moneyDestroyed * 12)) : 0; // Rough yearly projection denominator


            // --- B. ENGAGEMENT & PACING ---
            var activeUsersLast30d = users.Count(u => u.LastActivityAt >= thirtyDaysAgo);
            double engagementScore = (double)activeUsersLast30d / totalUsers * 100.0;
            
            // Social: % users in a group
            var usersInGroups = users.Count(u => u.GroupId != null);
            double socialScore = totalUsers > 0 ? (double)usersInGroups / totalUsers * 100.0 : 0;

            // Pacing: Check if users are hitting "End Game" too fast.
            // Theoretical max XP needed?
            // Let's grab the highest Level requirement.
            // For simplicity, let's say Level 10 is max, and requires 10,000 XP.
            // This really depends on 'Levels' configuration which might be hardcoded as formula.
            // Let's deduce PACE from Strategy.CycleDuration.
            // If Cycle = 12 months. Users should earn ~8% progress per month.
            
            // Let's calculate purely statistical "Inequality" (Status Engine)
            var top10PercentCount = Math.Max(1, totalUsers / 10);
            var topUsers = users.OrderByDescending(u => u.CurrentXp).Take(top10PercentCount).ToList();
            var bottomUsers = users.OrderByDescending(u => u.CurrentXp).Skip(top10PercentCount).ToList();
            
            double topAvgXp = topUsers.Any() ? topUsers.Average(u => u.CurrentXp) : 0;
            double bottomAvgXp = bottomUsers.Any() ? bottomUsers.Average(u => u.CurrentXp) : 0;
            
            double inequalityScore = bottomAvgXp > 0 ? topAvgXp / bottomAvgXp : 0; // Higher = More status/elite focus.

            // 3. Assemble DTO
            var dto = new SystemHealthDto
            {
                UserCount = totalUsers,
                MaxUsers = 100, // Should come from Establishment
                CycleDurationMonths = strategy.CycleDurationMonths,
                
                // Economy
                InflationRate = Math.Round(inflationRate, 2),
                HoardingRate = Math.Round(hoardingRate, 2),
                TotalStoreValue = orders.Sum(o => o.PricePaid), 
                
                
                // Engagement / Social
                SocialEngagementScore = Math.Round(socialScore, 2),
                StatusInequalityScore = Math.Round(inequalityScore, 2),
                PacingScore = Math.Round(engagementScore, 2), // Using Engagement as rough proxy for pacing health for now
                
                // Calculated Scores
                RealHealthScore = (int)Math.Clamp(
                    (engagementScore * 0.4) + // 40% importance on engagement
                    ((1.0 / (inflationRate + 0.1)) * 30.0) + // Economy stability
                    30 // Baseline
                , 0, 100),
                HealthScore = 85, // Theoretical baseline
                
                Advice = GenerateAdvice(inflationRate, hoardingRate, engagementScore, inequalityScore, strategy)
            };

            // Fix Store Value (Value of available items)
            var storeItems = await _context.StoreItems
                .Where(si => si.EstablishmentId == establishmentId && si.IsActive)
                .ToListAsync();
            dto.TotalStoreValue = storeItems.Sum(si => si.Price * (si.Stock == -1 ? 10 : si.Stock)); // Approx for infinite

            // Richest User
            var richest = users.OrderByDescending(u => u.Wallets.Sum(w => w.Balance)).FirstOrDefault();
            if (richest != null)
            {
                dto.RichestUserName = richest.FirstName + " " + richest.Username;
                dto.RichestUserWealth = (int)richest.Wallets.Sum(w => w.Balance);
            }

            return dto;
        }

        private List<string> GenerateAdvice(double inflation, double hoarding, double engagement, double inequality, GamificationStrategyDto strategy)
        {
            var advice = new List<string>();

            if (engagement < 50) advice.Add("L'engagement est faible. Vérifiez si vos récompenses sont attractives.");
            
            if (strategy.EngagementEngine == EngagementEngine.Social)
            {
                advice.Add("En mode Social, favorisez les défis de groupe pour booster l'engagement.");
            }
            
            if (inflation > 1.5) advice.Add("Inflation élevée : Les joueurs gagnent beaucoup plus qu'ils ne dépensent. Augmentez les prix boutique.");
            if (inflation < 0.5) advice.Add("Déflation / Avarice : Les prix sont peut-être trop élevés, les joueurs ne dépensent pas.");

            if (hoarding > 0.8) advice.Add("Thésaurisation massive : Vos joueurs accumulent l'argent. Créez des événements de dépense (Soldes, Items temporaires).");

            if (strategy.EngagementEngine == EngagementEngine.Status && inequality < 2.0)
            {
                advice.Add("En mode Statut, l'écart entre le Top 10% et les autres est trop faible. Créez des badges 'Élite' difficiles.");
            }

            return advice;
        }
    }
}
