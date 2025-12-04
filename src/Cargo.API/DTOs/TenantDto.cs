namespace Cargo.API.DTOs;

/// <summary>
/// DTO для тенанта
/// </summary>
public class TenantDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string TenantCode { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO для создания тенанта
/// </summary>
public class CreateTenantDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string TenantCode { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }
}

/// <summary>
/// DTO для обновления тенанта
/// </summary>
public class UpdateTenantDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }
}


