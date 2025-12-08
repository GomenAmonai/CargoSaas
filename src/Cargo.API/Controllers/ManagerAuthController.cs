using System.Text.Json;
using Cargo.API.DTOs;
using Cargo.Core;
using Cargo.Core.Entities;
using Cargo.Core.Enums;
using Cargo.Core.Exceptions;
using Cargo.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cargo.API.Controllers;

/// <summary>
/// Контроллер для аутентификации менеджеров через Telegram Login Widget
/// Использует тот же механизм валидации HMAC-SHA256 что и WebApp
/// </summary>
[ApiController]
[Route("api/manager/auth")]
public class ManagerAuthController : ControllerBase
{
    private readonly ITelegramAuthService _telegramAuthService;
    private readonly IJwtService _jwtService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ManagerAuthController> _logger;

    public ManagerAuthController(
        ITelegramAuthService telegramAuthService,
        IJwtService jwtService,
        UserManager<AppUser> userManager,
        ILogger<ManagerAuthController> logger)
    {
        _telegramAuthService = telegramAuthService;
        _jwtService = jwtService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Аутентификация менеджера через Telegram Login Widget
    /// Поддерживает два формата:
    /// 1. initData (string) - от Telegram WebApp
    /// 2. id, hash, auth_date и др - от Telegram Login Widget
    /// </summary>
    [HttpPost("telegram")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AuthenticateWithTelegram(
        [FromBody] TelegramLoginDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Валидируем Telegram данные (HMAC-SHA256)
            if (!ValidateTelegramLoginWidget(request))
            {
                _logger.LogWarning("Invalid Telegram Login Widget data received");
                throw new UnauthorizedException("Invalid Telegram authentication data");
            }

            _logger.LogInformation("Authenticating manager via Telegram: TelegramId={TelegramId}, Username={Username}", 
                request.Id, request.Username);

            // Ищем пользователя по TelegramId (.IgnoreQueryFilters() как и для Client)
            var user = await _userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.TelegramId == request.Id, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User with TelegramId={TelegramId} not found", request.Id);
                throw new UnauthorizedException("User not found. Please contact administrator to create your account.");
            }

            // КРИТИЧНАЯ ПРОВЕРКА: только Manager и SystemAdmin могут входить
            if (user.Role != UserRole.Manager && user.Role != UserRole.SystemAdmin)
            {
                _logger.LogWarning("User with TelegramId={TelegramId} attempted manager login with role={Role}", 
                    request.Id, user.Role);
                throw new ForbiddenException("Access denied. This endpoint is for managers only.");
            }

            // Обновляем данные пользователя из Telegram
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhotoUrl = request.PhotoUrl;
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError("Failed to update manager user: {Errors}", 
                    string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                throw new BusinessException("Failed to update user information");
            }

            // Генерируем JWT токен
            var token = _jwtService.GenerateToken(user);

            var response = new AuthResponseDto
            {
                Token = token,
                UserId = Guid.Parse(user.Id),
                TenantId = user.TenantId ?? Guid.Empty,
                FirstName = user.FirstName ?? string.Empty,
                Username = request.Username,
                PhotoUrl = user.PhotoUrl,
                Role = user.Role.ToString(),
                IsNewUser = false
            };

            _logger.LogInformation("Manager successfully authenticated: UserId={UserId}, Role={Role}, TenantId={TenantId}", 
                user.Id, user.Role, user.TenantId);

            return Ok(response);
        }
        catch (CargoException)
        {
            // Пробрасываем наши кастомные исключения (обработает middleware)
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during manager authentication");
            throw new BusinessException("Internal error during authentication");
        }
    }

    /// <summary>
    /// Валидация данных от Telegram Login Widget
    /// Используется HMAC-SHA256 для проверки подписи
    /// </summary>
    private bool ValidateTelegramLoginWidget(TelegramLoginDto data)
    {
        try
        {
            // Проверяем что все обязательные поля присутствуют
            if (data.Id <= 0 || 
                string.IsNullOrEmpty(data.FirstName) || 
                string.IsNullOrEmpty(data.Hash) || 
                data.AuthDate <= 0)
            {
                _logger.LogWarning("Missing required fields in Login Widget data");
                return false;
            }

            // Проверяем что данные не устарели (auth_date не старше 24 часов)
            var authDateTime = DateTimeOffset.FromUnixTimeSeconds(data.AuthDate);
            var now = DateTimeOffset.UtcNow;
            if (now - authDateTime > TimeSpan.FromHours(24))
            {
                _logger.LogWarning("Login Widget data expired. AuthDate: {AuthDate}, Now: {Now}", 
                    authDateTime, now);
                return false;
            }

            // Формируем словарь данных для валидации (все поля кроме hash)
            var dataDict = new Dictionary<string, string>
            {
                { "auth_date", data.AuthDate.ToString() },
                { "first_name", data.FirstName },
                { "id", data.Id.ToString() }
            };

            if (!string.IsNullOrEmpty(data.LastName))
            {
                dataDict["last_name"] = data.LastName;
            }

            if (!string.IsNullOrEmpty(data.PhotoUrl))
            {
                dataDict["photo_url"] = data.PhotoUrl;
            }

            if (!string.IsNullOrEmpty(data.Username))
            {
                dataDict["username"] = data.Username;
            }

            // Валидируем через HMAC-SHA256
            var isValid = _telegramAuthService.ValidateLoginWidget(dataDict, data.Hash);

            if (!isValid)
            {
                _logger.LogWarning("HMAC-SHA256 validation failed for Login Widget data. TelegramId: {TelegramId}", 
                    data.Id);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Telegram Login Widget data");
            return false;
        }
    }

    /// <summary>
    /// Получить информацию о текущем пользователе (для проверки токена)
    /// </summary>
    [HttpGet("me")]
    [Authorize(Roles = "Manager,SystemAdmin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new UnauthorizedException("User not found");
        }

        var userDto = new UserDto
        {
            Id = Guid.Parse(user.Id),
            TelegramId = user.TelegramId ?? 0,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName,
            Username = user.UserName,
            PhotoUrl = user.PhotoUrl,
            Role = user.Role.ToString(),
            TenantId = user.TenantId
        };

        return Ok(userDto);
    }
}

/// <summary>
/// DTO для Telegram Login Widget
/// Поля соответствуют callback данным от Telegram
/// </summary>
public class TelegramLoginDto
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public long Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("username")]
    public string? Username { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("photo_url")]
    public string? PhotoUrl { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("auth_date")]
    public long AuthDate { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;
}
