using System.Security.Cryptography;
using System.Text;
using Cargo.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cargo.Infrastructure.Services;

/// <summary>
/// Сервис для аутентификации через Telegram WebApp
/// Документация: https://core.telegram.org/bots/webapps#validating-data-received-via-the-mini-app
/// </summary>
public class TelegramAuthService : ITelegramAuthService
{
    private readonly string _botToken;
    private readonly ILogger<TelegramAuthService> _logger;

    public TelegramAuthService(IConfiguration configuration, ILogger<TelegramAuthService> logger)
    {
        _botToken = configuration["Telegram:BotToken"] 
            ?? throw new InvalidOperationException("Telegram:BotToken is not configured");
        _logger = logger;
    }

    public bool ValidateInitData(string initData)
    {
        try
        {
            // Логируем ПОЛНЫЙ initData для debug
            _logger.LogWarning("Raw initData (full): {InitData}", initData);
            
            // Для валидации НЕ декодируем URL - используем оригинальные значения!
            var data = ParseInitDataForValidation(initData);
            
            if (!data.TryGetValue("hash", out var receivedHash))
            {
                _logger.LogWarning("Hash not found in initData");
                return false;
            }

            // Убираем hash и signature из данных для проверки
            // signature - это новое поле Telegram WebApp, оно не участвует в валидации
            data.Remove("hash");
            data.Remove("signature");

            // Сортируем ключи и создаем data-check-string
            var sortedData = data.OrderBy(x => x.Key).ToList();
            var dataCheckString = string.Join("\n", 
                sortedData.Select(x => $"{x.Key}={x.Value}"));
            
            // Debug: покажем каждый параметр отдельно
            _logger.LogWarning("Sorted parameters ({Count}):", sortedData.Count);
            foreach (var item in sortedData)
            {
                _logger.LogWarning("  {Key} = {Value}", item.Key, item.Value.Length > 100 ? item.Value.Substring(0, 100) + "..." : item.Value);
            }

            // Создаем secret_key = HMAC-SHA256("WebAppData", bot_token)
            var secretKey = ComputeHmacSha256Bytes("WebAppData", _botToken);

            // Вычисляем hash = HMAC-SHA256(data-check-string, secret_key)
            var computedHash = ComputeHmacSha256WithBytes(dataCheckString, secretKey);

            var isValid = computedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
            
            if (!isValid)
            {
                _logger.LogWarning("InitData validation failed. Computed hash: {ComputedHash}, Received hash: {ReceivedHash}", 
                    computedHash, receivedHash);
                _logger.LogWarning("Data check string: {DataCheckString}", dataCheckString);
                _logger.LogWarning("Bot token length: {TokenLength}", _botToken?.Length ?? 0);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating initData");
            return false;
        }
    }

    // Парсинг для валидации - НЕ декодируем URL! Используем оригинальные значения
    private Dictionary<string, string> ParseInitDataForValidation(string initData)
    {
        var result = new Dictionary<string, string>();
        
        if (string.IsNullOrWhiteSpace(initData))
            return result;

        var pairs = initData.Split('&');
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2);
            if (keyValue.Length == 2)
            {
                // НЕ ДЕКОДИРУЕМ для валидации - Telegram подписывает URL-encoded данные!
                var key = keyValue[0];
                var value = keyValue[1];
                result[key] = value;
            }
        }

        return result;
    }

    // Парсинг для использования - декодируем URL
    public Dictionary<string, string> ParseInitData(string initData)
    {
        var result = new Dictionary<string, string>();
        
        if (string.IsNullOrWhiteSpace(initData))
            return result;

        var pairs = initData.Split('&');
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2);
            if (keyValue.Length == 2)
            {
                var key = Uri.UnescapeDataString(keyValue[0]);
                var value = Uri.UnescapeDataString(keyValue[1]);
                result[key] = value;
            }
        }

        return result;
    }

    private string ComputeHmacSha256(string data, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private byte[] ComputeHmacSha256Bytes(string data, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA256(keyBytes);
        return hmac.ComputeHash(dataBytes);
    }

    private string ComputeHmacSha256WithBytes(string data, byte[] keyBytes)
    {
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
