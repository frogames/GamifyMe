using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GamifyMe.Api.Data
{
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<DataContext>();
            // Hardcoded for migration debugging
            var connectionString = "Host=localhost;Database=gamifyme_db;Username=postgres;Password=Canon060570"; 
            Console.WriteLine("DataContextFactory: Creating DbContext...");

            builder.UseNpgsql(connectionString);

            return new DataContext(builder.Options, null);
        }
    }
}
