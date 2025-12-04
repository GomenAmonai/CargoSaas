namespace Cargo.API.DTOs;

/// <summary>
/// DTO для ответа на запрос аутентификации
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// JWT токен
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// ID пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// ID тенанта
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Username пользователя
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// URL фото профиля
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Это новый пользователь?
    /// </summary>
    public bool IsNewUser { get; set; }
}

