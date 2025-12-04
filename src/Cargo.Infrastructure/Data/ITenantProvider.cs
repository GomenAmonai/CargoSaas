namespace Cargo.Infrastructure.Data;

/// <summary>
/// Провайдер для получения текущего TenantId
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Получить идентификатор текущего тенанта
    /// </summary>
    Guid GetCurrentTenantId();

    /// <summary>
    /// Установить идентификатор текущего тенанта (для тестирования или миграций)
    /// </summary>
    void SetCurrentTenantId(Guid tenantId);
}


