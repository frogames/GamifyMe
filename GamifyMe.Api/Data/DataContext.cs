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
        public DbSet<ObjectiveObjective> ObjectiveObjectives { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- CORRECTION : Configuration Many-to-Many (Objectif -> Prérequis) ---
            // On dit à EF que Prerequisites est lié à IsPrerequisiteFor
            // en utilisant la table de jointure ObjectiveObjective
            modelBuilder.Entity<Objective>()
                .HasMany(o => o.Prerequisites)
                .WithMany(o => o.IsPrerequisiteFor) // <-- LA CORRECTION CLÉ
                .UsingEntity<ObjectiveObjective>(
                    // L'entité de droite (Prerequisites)
                    j => j
                        .HasOne(oo => oo.PrerequisiteObjective)
                        .WithMany() // Pas besoin de navigation retour sur la jointure
                        .HasForeignKey(oo => oo.PrerequisitesId)
                        .OnDelete(DeleteBehavior.Cascade),
                    // L'entité de gauche (IsPrerequisiteFor)
                    j => j
                        .HasOne(oo => oo.IsPrerequisiteForObjective)
                        .WithMany() // Pas besoin de navigation retour sur la jointure
                        .HasForeignKey(oo => oo.IsPrerequisiteForId)
                        .OnDelete(DeleteBehavior.Cascade),
                    // Configuration de la table de jointure
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