using System.Text.Json;
using Cargo.API.DTOs;
using Cargo.Core.Entities;
using Cargo.Core.Enums;
using Cargo.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cargo.API.Controllers;

/// <summary>
/// Контроллер для аутентификации клиентов через Telegram WebApp
/// </summary>
[ApiController]
[Route("api/client")]
public class ClientAuthController : ControllerBase
{
    private readonly ITelegramAuthService _telegramAuthService;
    private readonly IJwtService _jwtService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClientAuthController> _logger;

    public ClientAuthController(
        ITelegramAuthService telegramAuthService,
        IJwtService jwtService,
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        ILogger<ClientAuthController> logger)
    {
        _telegramAuthService = telegramAuthService;
        _jwtService = jwtService;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Аутентификация пользователя через Telegram WebApp
    /// </summary>
    /// <param name="request">Запрос с initData от Telegram</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>JWT токен и данные пользователя</returns>
    [HttpPost("auth")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Authenticate(
        [FromBody] AuthRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Валидируем initData от Telegram
            if (!_telegramAuthService.ValidateInitData(request.InitData))
            {
                _logger.LogWarning("Invalid initData received");
                return Unauthorized(new { message = "Invalid Telegram authentication data" });
            }

            // Парсим данные пользователя из initData
            var initDataDict = _telegramAuthService.ParseInitData(request.InitData);
            
            if (!initDataDict.TryGetValue("user", out var userJson))
            {
                _logger.LogWarning("User data not found in initData");
                return BadRequest(new { message = "User data not found in initData" });
            }

            // Десериализуем данные пользователя
            var telegramUser = JsonSerializer.Deserialize<TelegramUserData>(userJson);
            if (telegramUser == null)
            {
                _logger.LogWarning("Failed to deserialize user data");
                return BadRequest(new { message = "Invalid user data format" });
            }

            _logger.LogInformation("Authenticating Telegram user: {TelegramId}, Username: {Username}", 
                telegramUser.Id, telegramUser.Username);

            // Ищем пользователя по TelegramId
            // ВАЖНО: IgnoreQueryFilters() т.к. при логине у пользователя еще нет токена,
            // и HttpContextTenantProvider вернет Guid.Empty, что не позволит найти юзера
            var user = await _userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.TelegramId == telegramUser.Id, cancellationToken);
            
            var isNewUser = user == null;

            if (isNewUser)
            {
                // Создаем нового пользователя через UserManager
                user = new AppUser
                {
                    // Identity обязательные поля
                    UserName = $"tg_{telegramUser.Id}", // Уникальный username
                    Email = null, // У Telegram клиентов нет email
                    
                    // Telegram данные
                    TelegramId = telegramUser.Id,
                    FirstName = telegramUser.FirstName,
                    LastName = telegramUser.LastName,
                    PhotoUrl = telegramUser.PhotoUrl,
                    LanguageCode = telegramUser.LanguageCode,
                    IsPremium = telegramUser.IsPremium,
                    
                    // Роль и статус
                    Role = UserRole.Client,
                    
                    // Timestamps
                    LastLoginAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    
                    // TenantId будет установлен автоматически через TenantProvider
                    TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111") // Тестовый тенант
                };

                // Генерируем ClientCode для нового пользователя
                user.ClientCode = await GenerateUniqueClientCodeAsync(cancellationToken);

                var result = await _userManager.CreateAsync(user);
                
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create user: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return BadRequest(new { message = "Failed to create user", errors = result.Errors });
                }

                _logger.LogInformation("Created new user with TelegramId: {TelegramId}, UserId: {UserId}", 
                    telegramUser.Id, user.Id);
            }
            else
            {
                // Обновляем существующего пользователя
                user.FirstName = telegramUser.FirstName;
                user.LastName = telegramUser.LastName;
                user.PhotoUrl = telegramUser.PhotoUrl;
                user.LanguageCode = telegramUser.LanguageCode;
                user.IsPremium = telegramUser.IsPremium;
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                // Назначаем ClientCode, если его еще нет (старые пользователи до миграции)
                if (string.IsNullOrWhiteSpace(user.ClientCode))
                {
                    user.ClientCode = await GenerateUniqueClientCodeAsync(cancellationToken);
                }

                var result = await _userManager.UpdateAsync(user);
                
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to update user: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return BadRequest(new { message = "Failed to update user", errors = result.Errors });
                }

                _logger.LogInformation("Updated existing user with TelegramId: {TelegramId}, UserId: {UserId}", 
                    telegramUser.Id, user.Id);
            }

            // Для новых пользователей создаем демо-треки, если их еще нет
            if (isNewUser)
            {
                await EnsureDemoTracksForClientAsync(user, cancellationToken);
            }

            // Генерируем JWT токен
            var token = _jwtService.GenerateToken(user);

            var response = new AuthResponseDto
            {
                Token = token,
                UserId = Guid.Parse(user.Id),
                TenantId = user.TenantId ?? Guid.Empty,
                ClientCode = user.ClientCode,
                FirstName = user.FirstName ?? string.Empty,
                Username = telegramUser.Username,
                PhotoUrl = user.PhotoUrl,
                Role = user.Role.ToString(),
                IsNewUser = isNewUser
            };

            _logger.LogInformation("Successfully authenticated user: {UserId}, TelegramId: {TelegramId}", 
                user.Id, user.TelegramId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return StatusCode(500, new { message = "Internal server error during authentication" });
        }
    }

    /// <summary>
    /// Генерация уникального ClientCode для клиента
    /// Формат: CLT-XXXXXXXX (строчные/заглавные буквы и цифры)
    /// </summary>
    private async Task<string> GenerateUniqueClientCodeAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var raw = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var candidate = $"CLT-{raw}";

            var exists = await _userManager.Users
                .IgnoreQueryFilters()
                .AnyAsync(u => u.ClientCode == candidate, cancellationToken);

            if (!exists)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Failed to generate unique ClientCode after several attempts");
    }

    /// <summary>
    /// Создает несколько демо-треков для нового клиента, если у него еще нет треков
    /// </summary>
    private async Task EnsureDemoTracksForClientAsync(AppUser user, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.ClientCode) || !user.TenantId.HasValue)
        {
            return;
        }

        try
        {
            var existingTracks = await _unitOfWork.Tracks
                .GetByClientCodeAsync(user.ClientCode, cancellationToken);

            if (existingTracks.Any())
            {
                return;
            }

            var now = DateTime.UtcNow;
            var tenantId = user.TenantId.Value;

            var demoTracks = new List<Track>
            {
                new Track
                {
                    TenantId = tenantId,
                    ClientCode = user.ClientCode,
                    TrackingNumber = $"DEMO-{user.ClientCode}-1",
                    Status = TrackStatus.Created,
                    Description = "Demo: New shipment created",
                    OriginCountry = "China",
                    DestinationCountry = "Russia",
                    Weight = 1.2m,
                    CreatedAt = now.AddDays(-2),
                    EstimatedDeliveryAt = now.AddDays(5)
                },
                new Track
                {
                    TenantId = tenantId,
                    ClientCode = user.ClientCode,
                    TrackingNumber = $"DEMO-{user.ClientCode}-2",
                    Status = TrackStatus.InTransit,
                    Description = "Demo: Package is on the way",
                    OriginCountry = "Turkey",
                    DestinationCountry = "Russia",
                    Weight = 3.5m,
                    CreatedAt = now.AddDays(-5),
                    ShippedAt = now.AddDays(-3),
                    EstimatedDeliveryAt = now.AddDays(2)
                },
                new Track
                {
                    TenantId = tenantId,
                    ClientCode = user.ClientCode,
                    TrackingNumber = $"DEMO-{user.ClientCode}-3",
                    Status = TrackStatus.Delivered,
                    Description = "Demo: Recently delivered shipment",
                    OriginCountry = "Germany",
                    DestinationCountry = "Russia",
                    Weight = 0.8m,
                    CreatedAt = now.AddDays(-10),
                    ShippedAt = now.AddDays(-8),
                    EstimatedDeliveryAt = now.AddDays(-3),
                    ActualDeliveryAt = now.AddDays(-2)
                }
            };

            foreach (var track in demoTracks)
            {
                await _unitOfWork.Tracks.AddAsync(track, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created {Count} demo tracks for client {ClientCode} (UserId: {UserId})",
                demoTracks.Count,
                user.ClientCode,
                user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create demo tracks for client {ClientCode} (UserId: {UserId})",
                user.ClientCode,
                user.Id);
            // Не ломаем логин, если сидер упал
        }
    }

    /// <summary>
    /// Вспомогательный класс для десериализации данных пользователя от Telegram
    /// </summary>
    private class TelegramUserData
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
        
        [System.Text.Json.Serialization.JsonPropertyName("language_code")]
        public string? LanguageCode { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("is_premium")]
        public bool IsPremium { get; set; }
    }
}
