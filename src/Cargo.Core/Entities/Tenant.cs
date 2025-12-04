namespace Cargo.Core.Entities;

/// <summary>
/// Сущность тенанта (компания/организация)
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>
    /// Название компании
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Уникальный код тенанта (для использования в URL, API и т.д.)
    /// </summary>
    public string TenantCode { get; set; } = string.Empty;

    /// <summary>
    /// Email контактного лица
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// Телефон контактного лица
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Активен ли тенант
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Дата истечения подписки
    /// </summary>
    public DateTime? SubscriptionExpiresAt { get; set; }

    /// <summary>
    /// Треки, принадлежащие этому тенанту
    /// </summary>
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}

