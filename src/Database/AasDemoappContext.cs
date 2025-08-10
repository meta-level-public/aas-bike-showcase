using AasDemoapp.Database.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace AasDemoapp.Database
{

    public class AasDemoappContext : DbContext
    {
        public DbSet<ImportedShell> ImportedShells { get; set; }
        public DbSet<KatalogEintrag> KatalogEintraege { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<Setting> Settings { get; set; }
        public DbSet<UpdateableShell> UpdateableShells { get; set; }
        public DbSet<ConfiguredProduct> ConfiguredProducts { get; set; }
        public DbSet<ProducedProduct> ProducedProducts { get; set; }
        public DbSet<ProductPart> ProductParts { get; set; }

        public string DbPath { get; }

        public AasDemoappContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var directory = Path.Join(path, "AasDemoapp");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            DbPath = Path.Join(directory, "AasDemoapp.db");
        }

        public AasDemoappContext(DbContextOptions<AasDemoappContext> options) : base(options)
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var directory = Path.Join(path, "AasDemoapp");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            DbPath = Path.Join(directory, "AasDemoapp.db");
            Console.WriteLine(DbPath);
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ConfiguredProduct -> ProducedProducts (1:N)
            modelBuilder.Entity<ConfiguredProduct>()
                .HasMany(e => e.ProducedProducts)
                .WithOne(e => e.ConfiguredProduct)
                .HasForeignKey(e => e.ConfiguredProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(true);

            // ConfiguredProduct -> ProductParts/Bestandteile (1:N)
            modelBuilder.Entity<ConfiguredProduct>()
                .HasMany(e => e.Bestandteile)
                .WithOne(e => e.ConfiguredProduct)
                .HasForeignKey(e => e.ConfiguredProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            // ProducedProduct -> ProductParts/Bestandteile (1:N)
            modelBuilder.Entity<ProducedProduct>()
                .HasMany(e => e.Bestandteile)
                .WithOne(e => e.ProducedProduct)
                .HasForeignKey(e => e.ProducedProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            // ProductPart -> KatalogEintrag (N:1)
            modelBuilder.Entity<ProductPart>()
                .HasOne(e => e.KatalogEintrag)
                .WithMany()
                .HasForeignKey(e => e.KatalogEintragId)
                .OnDelete(DeleteBehavior.Restrict) // Verhindert Löschen von KatalogEintrag wenn ProductPart existiert
                .IsRequired(true);

            // KatalogEintrag -> ReferencedType (Self-Reference)
            modelBuilder.Entity<KatalogEintrag>()
                .HasOne(e => e.ReferencedType)
                .WithMany()
                .HasForeignKey(e => e.ReferencedTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            // KatalogEintrag -> ConfiguredProducts (N:M via ProductParts)
            modelBuilder.Entity<KatalogEintrag>()
                .HasMany(e => e.ConfiguredProducts)
                .WithMany()
                .UsingEntity<ProductPart>(
                    j => j.HasOne(pp => pp.ConfiguredProduct)
                        .WithMany(cp => cp.Bestandteile)
                        .HasForeignKey(pp => pp.ConfiguredProductId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne(pp => pp.KatalogEintrag)
                        .WithMany()
                        .HasForeignKey(pp => pp.KatalogEintragId)
                        .OnDelete(DeleteBehavior.Restrict)
                );

            // UpdateableShell -> KatalogEintrag (1:1)
            modelBuilder.Entity<UpdateableShell>()
                .HasOne(e => e.KatalogEintrag)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(true);

            // KatalogEintrag -> Supplier (N:1)
            modelBuilder.Entity<KatalogEintrag>()
                .HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict) // Verhindert Löschen von Supplier wenn KatalogEintrag existiert
                .IsRequired(false);

            modelBuilder.Entity<Supplier>()
                .Property(e => e.SecuritySetting)
                .HasConversion(
                    v => DBJsonConverter.Serialize(v),
                    v => DBJsonConverter.Deserialize<SecuritySetting>(v));

            // Global Query Filter für Soft Delete
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var body = Expression.Equal(
                        Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)),
                        Expression.Constant(false));
                    var lambda = Expression.Lambda(body, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

        }

        public override int SaveChanges()
        {
            HandleSoftDelete();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleSoftDelete();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void HandleSoftDelete()
        {
            var deletedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in deletedEntries)
            {
                if (entry.Entity is ISoftDelete softDeleteEntity)
                {
                    // Statt echtem Löschen: Soft Delete markieren
                    entry.State = EntityState.Modified;
                    softDeleteEntity.IsDeleted = true;
                    softDeleteEntity.DeletedAt = DateTime.UtcNow;
                }
            }
        }

        /// <summary>
        /// Gibt ein IQueryable zurück, das auch gelöschte Entitäten enthält (ohne Soft Delete Filter)
        /// </summary>
        /// <typeparam name="T">Der Entitätstyp</typeparam>
        /// <returns>IQueryable mit allen Entitäten (auch gelöschte)</returns>
        public IQueryable<T> SetIgnoreQueryFilters<T>() where T : class
        {
            return Set<T>().IgnoreQueryFilters();
        }

        /// <summary>
        /// Hard Delete: Löscht eine Entität permanent aus der Datenbank
        /// </summary>
        /// <typeparam name="T">Der Entitätstyp</typeparam>
        /// <param name="entity">Die zu löschende Entität</param>
        public void HardDelete<T>(T entity) where T : class
        {
            if (entity is ISoftDelete softDeleteEntity)
            {
                // Temporär den Query Filter umgehen
                var entry = Entry(entity);
                entry.State = EntityState.Deleted;
            }
            else
            {
                Remove(entity);
            }
        }
    }
}