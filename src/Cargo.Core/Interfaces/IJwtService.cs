using Cargo.Core.Entities;

namespace Cargo.Core.Interfaces;

/// <summary>
/// Сервис для работы с JWT токенами
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Генерирует JWT токен для пользователя
    /// </summary>
    string GenerateToken(AppUser user);

    /// <summary>
    /// Валидирует JWT токен
    /// </summary>
    bool ValidateToken(string token);
}

