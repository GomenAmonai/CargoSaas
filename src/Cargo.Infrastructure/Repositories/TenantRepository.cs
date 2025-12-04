using Cargo.Core.Entities;
using Cargo.Core.Interfaces;
using Cargo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cargo.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с тенантами
/// </summary>
public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(CargoDbContext context) : base(context)
    {
    }

    public async Task<Tenant?> GetByTenantCodeAsync(
        string tenantCode,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(t => t.TenantCode == tenantCode, cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetActivTenantsAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.IsActive)
            .OrderBy(t => t.CompanyName)
            .ToListAsync(cancellationToken);
    }

    // Переопределяем методы для работы без фильтра TenantId
    public override async Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);
    }

    public override async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
}


