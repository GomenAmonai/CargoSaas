using Cargo.Core.Entities;

namespace Cargo.Core.Interfaces;

/// <summary>
/// Репозиторий для работы с тенантами
/// </summary>
public interface ITenantRepository : IRepository<Tenant>
{
    /// <summary>
    /// Получить тенанта по коду
    /// </summary>
    Task<Tenant?> GetByTenantCodeAsync(string tenantCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить активных тенантов
    /// </summary>
    Task<IEnumerable<Tenant>> GetActivTenantsAsync(CancellationToken cancellationToken = default);
}

