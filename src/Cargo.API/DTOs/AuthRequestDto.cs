namespace Cargo.API.DTOs;

/// <summary>
/// DTO для запроса аутентификации через Telegram WebApp
/// </summary>
public class AuthRequestDto
{
    /// <summary>
    /// initData строка от Telegram WebApp
    /// </summary>
    public string InitData { get; set; } = string.Empty;
}

