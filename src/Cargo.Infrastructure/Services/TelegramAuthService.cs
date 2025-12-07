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
        _logger.LogInformation("TelegramAuthService initialized with configured bot token");
    }

    public bool ValidateInitData(string initData)
    {
        try
        {
            _logger.LogWarning("Raw initData (full): {InitData}", initData);
            
            // Парсим query string и СРАЗУ исключаем hash/signature
            var pairs = initData.Split('&');
            var data = new Dictionary<string, string>();
            string receivedHash = string.Empty;
            
            foreach (var pair in pairs)
            {
                var kv = pair.Split('=', 2);
                if (kv.Length != 2) continue;
                
                var key = kv[0];
                var value = kv[1];
                
                // Сохраняем hash отдельно
                if (key == "hash")
                {
                    receivedHash = value;
                    continue;
                }
                
                // signature не участвует в HMAC-проверке (WebApp 2.0 MiniApps)
                if (key == "signature")
                {
                    continue;
                }
                
                // ВАЖНО: для HMAC используем ДЕКОДИРОВАННЫЕ значения
                // Telegram в примере показывает data-check-string c декодированным user JSON
                var decodedValue = Uri.UnescapeDataString(value);
                data[key] = decodedValue;
            }
            
            if (string.IsNullOrEmpty(receivedHash))
            {
                _logger.LogWarning("Hash not found in initData");
                return false;
            }

            // Сортируем и создаем data-check-string
            var checkString = string.Join("\n", 
                data.OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"));
            
            _logger.LogWarning("Data check string for validation: {CheckString}", checkString);

            // Создаем secret_key = HMAC-SHA256("WebAppData", bot_token)
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("WebAppData"));
            var secretKey = hmac.ComputeHash(Encoding.UTF8.GetBytes(_botToken));

            // Вычисляем hash = HMAC-SHA256(data-check-string, secret_key)
            using var hashHmac = new HMACSHA256(secretKey);
            var hashBytes = hashHmac.ComputeHash(Encoding.UTF8.GetBytes(checkString));
            var computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            var isValid = computedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
            
            if (!isValid)
            {
                _logger.LogWarning("Validation FAILED. Computed: {Computed}, Received: {Received}", 
                    computedHash, receivedHash);
            }
            else
            {
                _logger.LogInformation("✅ InitData validation SUCCESS!");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating initData");
            return false;
        }
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
