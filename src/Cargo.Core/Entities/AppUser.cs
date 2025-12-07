using Cargo.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace Cargo.Core.Entities;

/// <summary>
/// Унифицированная сущность пользователя (Single Table Inheritance)
/// Поддерживает как Managers (email/password), так и Clients (Telegram)
/// </summary>
public class AppUser : IdentityUser
{
    /// <summary>
    /// ID тенанта, к которому принадлежит пользователь
    /// Null для SystemAdmin
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Telegram ID пользователя (для клиентов из Telegram)
    /// Null для Managers
    /// </summary>
    public long? TelegramId { get; set; }

    /// <summary>
    /// Уникальный код клиента для связи с треками
    /// Генерируется автоматически при регистрации
    /// Формат: "CLT-XXXXXXXX"
    /// </summary>
    public string? ClientCode { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// URL фото профиля (обычно от Telegram)
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Язык интерфейса пользователя (ru, en и т.д.)
    /// </summary>
    public string? LanguageCode { get; set; }

    /// <summary>
    /// Роль пользователя в системе
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Client;

    /// <summary>
    /// Является ли пользователь Telegram Premium
    /// </summary>
    public bool IsPremium { get; set; }

    /// <summary>
    /// Последний раз заходил в приложение
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Навигационное свойство к тенанту
    /// </summary>
    public Tenant? Tenant { get; set; }
}

