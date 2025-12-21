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
        public DbSet<StoreItem> StoreItems { get; set; }
        public DbSet<Establishment> Establishments { get; set; }
        public DbSet<Objective> Objectives { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<Group> Groups { get; set; }
        // public DbSet<GroupUser> GroupUsers { get; set; } // Removing if not exists, but let's check. 
        // Logic: Group.cs likely has Members. 
        // Let's stick to what we know exists or was there. 
        
        // I will use a safe replacement based on what I see in list_dir
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Validation> Validations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<UserInventory> UserInventories { get; set; }
        public DbSet<BonusPeriod> BonusPeriods { get; set; }
        public DbSet<UserObjective> UserObjectives { get; set; }
        public DbSet<UserBadge> UserBadges { get; set; }
        
        public DbSet<ContentKit> ContentKits { get; set; }
        public DbSet<KitRating> KitRatings { get; set; }
        // public DbSet<ObjectiveObjective> ObjectiveObjective { get; set; } // Removed to avoid conflict
 // Kept from original, not explicitly removed by snippet

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            CheckForTemplateDeletion();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            CheckForTemplateDeletion();
            return base.SaveChanges();
        }

        private void CheckForTemplateDeletion()
        {
            var deletedEstablishments = ChangeTracker.Entries<Establishment>()
                .Where(e => e.State == EntityState.Deleted && e.Entity.IsTemplate);

            if (deletedEstablishments.Any())
            {
                throw new InvalidOperationException("Impossible de supprimer un établissement marqué comme 'Modèle' (IsTemplate).");
            }
        }

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