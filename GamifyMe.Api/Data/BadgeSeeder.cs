using GamifyMe.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GamifyMe.Api.Data
{
    public static class BadgeSeeder
    {
        public static async Task SeedAsync(DataContext context)
        {
             // Legacy seeding removed as per user request.
             await Task.CompletedTask;
        }
    }
}
