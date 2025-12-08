using Cargo.Core;

namespace Cargo.Infrastructure.Data;

/// <summary>
/// Реализация провайдера тенанта (будет заменена на HttpContext-based в API)
/// Для MVP используем фиксированный тестовый тенант
/// </summary>
public class TenantProvider : ITenantProvider
{
    // Тестовый тенант для MVP (совпадает с seed данными)
    private Guid _currentTenantId = AppConstants.Tenants.TestTenantId;

    public Guid GetCurrentTenantId()
    {
        return _currentTenantId;
    }

    public void SetCurrentTenantId(Guid tenantId)
    {
        _currentTenantId = tenantId;
    }
}


