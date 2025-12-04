namespace Cargo.Core.Interfaces;

/// <summary>
/// Сервис для аутентификации через Telegram WebApp
/// </summary>
public interface ITelegramAuthService
{
    /// <summary>
    /// Валидирует initData от Telegram WebApp
    /// </summary>
    /// <param name="initData">Строка initData от Telegram</param>
    /// <returns>True если данные валидны</returns>
    bool ValidateInitData(string initData);

    /// <summary>
    /// Парсит initData и извлекает данные пользователя
    /// </summary>
    /// <param name="initData">Строка initData от Telegram</param>
    /// <returns>Словарь с данными пользователя</returns>
    Dictionary<string, string> ParseInitData(string initData);
}

