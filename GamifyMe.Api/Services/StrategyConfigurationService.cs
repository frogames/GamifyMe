using GamifyMe.Api.Data;
using GamifyMe.Shared.Dtos;
using GamifyMe.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GamifyMe.Api.Services
{
    public class StrategyConfigurationService
    {
        private readonly DataContext _context;

        public StrategyConfigurationService(DataContext context)
        {
            _context = context;
        }

        public async Task<GamificationStrategyDto> GetStrategyAsync(Guid establishmentId)
        {
            // Try to find existing strategy
            var strategy = await _context.GamificationStrategies
                .FirstOrDefaultAsync(s => s.EstablishmentId == establishmentId);

            if (strategy == null)
            {
                // Create default if not exists (Lazy Initialization)
                strategy = new GamificationStrategy
                {
                    EstablishmentId = establishmentId,
                    EngagementEngine = EngagementEngine.Mastery,
                    FeedbackFrequency = FeedbackFrequency.Immediate,
                    CycleType = CycleType.Infinite,
                    CycleDurationMonths = 12,
                    LastUpdated = DateTime.UtcNow
                };
                _context.GamificationStrategies.Add(strategy);
                await _context.SaveChangesAsync();
            }

            return new GamificationStrategyDto
            {
                EngagementEngine = strategy.EngagementEngine,
                FeedbackFrequency = strategy.FeedbackFrequency,
                CycleType = strategy.CycleType,
                CycleDurationMonths = strategy.CycleDurationMonths,
                ActivityDomain = strategy.ActivityDomain
            };
        }

        public async Task UpdateStrategyAsync(Guid establishmentId, GamificationStrategyDto dto)
        {
            var strategy = await _context.GamificationStrategies
                .FirstOrDefaultAsync(s => s.EstablishmentId == establishmentId);

            if (strategy == null)
            {
                strategy = new GamificationStrategy
                {
                    EstablishmentId = establishmentId
                };
                _context.GamificationStrategies.Add(strategy);
            }

            strategy.EngagementEngine = dto.EngagementEngine;
            strategy.FeedbackFrequency = dto.FeedbackFrequency;
            strategy.CycleType = dto.CycleType;
            strategy.CycleDurationMonths = dto.CycleDurationMonths;
            strategy.ActivityDomain = dto.ActivityDomain;
            strategy.LastUpdated = DateTime.UtcNow;

            // Also sync with Establishment settings if necessary
            var establishment = await _context.Establishments.FindAsync(establishmentId);
            if (establishment != null)
            {
                establishment.CycleDurationMonths = dto.CycleDurationMonths;
            }

            await _context.SaveChangesAsync();
        }
    }
}
