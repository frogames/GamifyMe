using GamifyMe.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GamifyMe.Api.Services
{
    public class InactiveAccountCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InactiveAccountCleanupService> _logger;

        public InactiveAccountCleanupService(IServiceProvider serviceProvider, ILogger<InactiveAccountCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("InactiveAccountCleanupService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("InactiveAccountCleanupService checking for inactive accounts...");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                        var establishments = await context.Establishments.ToListAsync(stoppingToken);

                        foreach (var establishment in establishments)
                        {
                            var delayDays = establishment.ArchiveUsersAfterInactiveDays;
                            if (delayDays <= 0) continue; // 0 or less means disabled

                            var thresholdDate = DateTime.UtcNow.AddDays(-delayDays);

                            var inactiveUsers = await context.Users
                                .Where(u => u.EstablishmentId == establishment.Id 
                                            && (u.LastActivityAt < thresholdDate && u.CreatedAt < thresholdDate) // Ensure we don't delete new users who haven't logged in yet if LastActivityAt is default
                                            && u.Role != "SuperAdmin" 
                                            && u.Role != "Admin") // Protect admins
                                .ToListAsync(stoppingToken);

                            if (inactiveUsers.Any())
                            {
                                _logger.LogInformation($"Found {inactiveUsers.Count} inactive users in establishment {establishment.Name} (ID: {establishment.Id}) to delete.");
                                
                                context.Users.RemoveRange(inactiveUsers);
                                await context.SaveChangesAsync(stoppingToken);
                                
                                _logger.LogInformation($"Deleted {inactiveUsers.Count} users.");
                            }
                        }
                    }

                    // Run once a day
                    // await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                    // For testing purposes or just standard implementation, waiting 24 hours is good. 
                    // However, to ensure it doesn't drift or miss, usually we calculate time to next run. 
                    // But simpler here: wait 24h.
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing InactiveAccountCleanupService.");
                    // Start again in 1 hour if failed
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }
    }
}
