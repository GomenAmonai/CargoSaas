namespace Cargo.Core.Entities;

/// <summary>
/// Базовая сущность для всех объектов с поддержкой multi-tenancy
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Уникальный идентификатор сущности
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор тенанта для multi-tenancy
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Дата создания записи
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата последнего обновления записи
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
}


