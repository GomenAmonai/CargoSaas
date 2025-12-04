namespace Cargo.Infrastructure.Data;

/// <summary>
/// Реализация провайдера тенанта (будет заменена на HttpContext-based в API)
/// </summary>
public class TenantProvider : ITenantProvider
{
    private Guid _currentTenantId = Guid.Empty;

    public Guid GetCurrentTenantId()
    {
        return _currentTenantId;
    }

    public void SetCurrentTenantId(Guid tenantId)
    {
        _currentTenantId = tenantId;
    }
}


