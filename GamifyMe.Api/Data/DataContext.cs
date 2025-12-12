using GamifyMe.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GamifyMe.Api.Data
{
    public class DataContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public DataContext(DbContextOptions<DataContext> options, IHttpContextAccessor? httpContextAccessor = null) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // --- Tables ---
        public DbSet<User> Users { get; set; }
        public DbSet<Establishment> Establishments { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Objective> Objectives { get; set; }
        public DbSet<Validation> Validations { get; set; }
        public DbSet<StoreItem> StoreItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<UserInventory> UserInventories { get; set; }
        // public DbSet<ObjectiveObjective> ObjectiveObjective { get; set; } // Removed to avoid conflict
        public DbSet<Group> Groups { get; set; }
        public DbSet<BonusPeriod> BonusPeriods { get; set; }
        public DbSet<UserObjective> UserObjectives { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<UserBadge> UserBadges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- CORRECTION : Configuration Many-to-Many (Objectif -> Prérequis) ---
            modelBuilder.Entity<Objective>()
                .HasMany(o => o.Prerequisites)
                .WithMany(o => o.IsPrerequisiteFor)
                .UsingEntity<ObjectiveObjective>(
                    l => l.HasOne(oo => oo.PrerequisiteObjective).WithMany().HasForeignKey(oo => oo.PrerequisitesId),
                    r => r.HasOne(oo => oo.IsPrerequisiteForObjective).WithMany().HasForeignKey(oo => oo.IsPrerequisiteForId),
                    j => j.HasKey(oo => new { oo.IsPrerequisiteForId, oo.PrerequisitesId })
                );



            // --- Configuration UserObjective ---
            modelBuilder.Entity<UserObjective>()
                .HasOne(uo => uo.User)
                .WithMany(u => u.UserObjectives)
                .HasForeignKey(uo => uo.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserObjective>()
                .HasOne(uo => uo.Objective)
                .WithMany(o => o.UserObjectives)
                .HasForeignKey(uo => uo.ObjectiveId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Configuration UserBadge ---
            modelBuilder.Entity<UserBadge>()
                .HasOne(ub => ub.User)
                .WithMany(u => u.UserBadges)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Filtre Global de Sécurité (Multi-Tenant) ---
            ApplyEstablishmentFilter(modelBuilder);
        }

        private void ApplyEstablishmentFilter(ModelBuilder modelBuilder)
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            {
                var establishmentIdClaim = user.FindFirstValue("EstablishmentId");
                if (Guid.TryParse(establishmentIdClaim, out var establishmentId))
                {
                    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                    {
                        if (typeof(IEstablishmentScoped).IsAssignableFrom(entityType.ClrType))
                        {
                            modelBuilder.Entity(entityType.ClrType)
                                .HasQueryFilter(Convert(establishmentId, entityType.ClrType));
                        }
                    }
                }
            }
        }

        private static System.Linq.Expressions.LambdaExpression Convert(Guid id, Type type)
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(type, "e");
            var property = System.Linq.Expressions.Expression.Property(parameter, "EstablishmentId");
            var constant = System.Linq.Expressions.Expression.Constant(id);
            var equal = System.Linq.Expressions.Expression.Equal(property, constant);
            return System.Linq.Expressions.Expression.Lambda(equal, parameter);
        }
    }
}