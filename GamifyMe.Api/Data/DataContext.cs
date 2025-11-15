using GamifyMe.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GamifyMe.Api.Data
{
    public class DataContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DataContext(DbContextOptions<DataContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
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
        public DbSet<ObjectiveObjective> ObjectiveObjectives { get; set; } // Table de jointure

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configuration Many-to-Many (Objectif -> Prérequis) ---
            modelBuilder.Entity<Objective>()
                .HasMany(o => o.Prerequisites)
                .WithMany()
                .UsingEntity<ObjectiveObjective>(
                    j => j
                        .HasOne(oo => oo.PrerequisiteObjective)
                        .WithMany()
                        .HasForeignKey(oo => oo.PrerequisitesId)
                        .OnDelete(DeleteBehavior.Cascade), // Si on supprime un prérequis, la liaison tombe
                    j => j
                        .HasOne(oo => oo.IsPrerequisiteForObjective)
                        .WithMany()
                        .HasForeignKey(oo => oo.IsPrerequisiteForId)
                        .OnDelete(DeleteBehavior.Cascade), // Si on supprime l'objectif, la liaison tombe
                    j =>
                    {
                        j.HasKey(oo => new { oo.IsPrerequisiteForId, oo.PrerequisitesId });
                        j.ToTable("ObjectiveObjectives");
                    });

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
                    // Appliquer le filtre à toutes les entités qui implémentent IEstablishmentScoped
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

        // Helper pour convertir l'expression lambda au bon type
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