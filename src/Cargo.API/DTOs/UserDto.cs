namespace Cargo.API.DTOs;

/// <summary>
/// DTO для пользователя
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public long TelegramId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? PhotoUrl { get; set; }
    public string? LanguageCode { get; set; }
    public bool IsPremium { get; set; }
    public string Role { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

