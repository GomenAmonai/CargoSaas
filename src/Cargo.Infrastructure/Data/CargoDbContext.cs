using Cargo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Cargo.Infrastructure.Data;

/// <summary>
/// Контекст базы данных для Cargo с поддержкой multi-tenancy
/// </summary>
public class CargoDbContext : DbContext
{
    private readonly Guid _currentTenantId;

    public CargoDbContext(DbContextOptions<CargoDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _currentTenantId = tenantProvider.GetCurrentTenantId();
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Track> Tracks => Set<Track>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Конфигурация сущностей
        ConfigureTenant(modelBuilder);
        ConfigureTrack(modelBuilder);

        // Применение глобального фильтра для multi-tenancy
        ApplyGlobalQueryFilters(modelBuilder);
    }

    /// <summary>
    /// Конфигурация сущности Tenant
    /// </summary>
    private void ConfigureTenant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.TenantCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(t => t.TenantCode)
                .IsUnique();

            entity.Property(t => t.ContactEmail)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(t => t.ContactPhone)
                .HasMaxLength(20);

            entity.Property(t => t.CreatedAt)
                .IsRequired();

            // Tenant сам себе тенант, но мы исключаем его из фильтра
            entity.HasQueryFilter(t => t.Id == t.TenantId);
        });
    }

    /// <summary>
    /// Конфигурация сущности Track
    /// </summary>
    private void ConfigureTrack(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.ClientCode)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(t => t.TrackingNumber)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(t => t.TrackingNumber);

            entity.HasIndex(t => new { t.TenantId, t.TrackingNumber })
                .IsUnique();

            entity.Property(t => t.Status)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(t => t.Description)
                .HasMaxLength(500);

            entity.Property(t => t.Weight)
                .HasPrecision(10, 2);

            entity.Property(t => t.DeclaredValue)
                .HasPrecision(18, 2);

            entity.Property(t => t.OriginCountry)
                .HasMaxLength(100);

            entity.Property(t => t.DestinationCountry)
                .HasMaxLength(100);

            entity.Property(t => t.Notes)
                .HasMaxLength(1000);

            entity.Property(t => t.CreatedAt)
                .IsRequired();

            // Связь с Tenant
            entity.HasOne(t => t.Tenant)
                .WithMany(tenant => tenant.Tracks)
                .HasForeignKey(t => t.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Применение глобального фильтра для multi-tenancy
    /// </summary>
    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Применяем фильтр ко всем сущностям, наследующим BaseEntity (кроме Tenant)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType) && 
                entityType.ClrType != typeof(Tenant))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var tenantIdProperty = Expression.Property(parameter, nameof(BaseEntity.TenantId));
                var tenantIdValue = Expression.Constant(_currentTenantId);
                var filter = Expression.Lambda(
                    Expression.Equal(tenantIdProperty, tenantIdValue),
                    parameter);

                var existingFilter = entityType.GetQueryFilter();
                if (existingFilter != null)
                {
                    var combinedFilter = Expression.Lambda(
                        Expression.AndAlso(existingFilter.Body, filter.Body),
                        parameter);
                    entityType.SetQueryFilter(combinedFilter);
                }
                else
                {
                    entityType.SetQueryFilter(filter);
                }
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Автоматическое проставление TenantId для новых сущностей
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added && e.Entity.TenantId == Guid.Empty);

        foreach (var entry in entries)
        {
            if (entry.Entity.GetType() != typeof(Tenant))
            {
                entry.Entity.TenantId = _currentTenantId;
            }
            else
            {
                // Для Tenant - TenantId = Id (сам себе тенант)
                entry.Entity.TenantId = entry.Entity.Id;
            }
        }

        // Автоматическое обновление UpdatedAt
        var updatedEntries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in updatedEntries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

