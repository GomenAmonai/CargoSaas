namespace Cargo.Infrastructure.Data;

/// <summary>
/// Реализация провайдера тенанта (будет заменена на HttpContext-based в API)
/// Для MVP используем фиксированный тестовый тенант
/// </summary>
public class TenantProvider : ITenantProvider
{
    // Тестовый тенант для MVP (совпадает с seed данными)
    private Guid _currentTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public Guid GetCurrentTenantId()
    {
        return _currentTenantId;
    }

    public void SetCurrentTenantId(Guid tenantId)
    {
        _currentTenantId = tenantId;
    }
}


