namespace Cargo.Core;

/// <summary>
/// Константы приложения для избежания magic strings и values
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Константы для Multi-tenancy
    /// </summary>
    public static class Tenants
    {
        /// <summary>
        /// ID тестового тенанта для MVP (TEST Cargo Company)
        /// В production будет создаваться через админку
        /// </summary>
        public static readonly Guid TestTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        
        /// <summary>
        /// Код тестового тенанта
        /// </summary>
        public const string TestTenantCode = "TEST";
    }

    /// <summary>
    /// Константы для ClientCode генерации
    /// </summary>
    public static class ClientCodes
    {
        /// <summary>
        /// Префикс для ClientCode (CLT-XXXXXXXX)
        /// </summary>
        public const string Prefix = "CLT-";
        
        /// <summary>
        /// Длина случайной части (8 символов)
        /// </summary>
        public const int RandomPartLength = 8;
        
        /// <summary>
        /// Максимальное количество попыток генерации уникального кода
        /// </summary>
        public const int MaxGenerationAttempts = 10;
    }

    /// <summary>
    /// Константы для TrackingNumber
    /// </summary>
    public static class TrackingNumbers
    {
        /// <summary>
        /// Префикс для демо-треков
        /// </summary>
        public const string DemoPrefix = "DEMO-";
    }

    /// <summary>
    /// Константы для JWT токенов
    /// </summary>
    public static class Jwt
    {
        /// <summary>
        /// Дефолтное время жизни токена (30 дней для MVP)
        /// В production лучше 15 минут + refresh token
        /// </summary>
        public const int DefaultExpirationMinutes = 43200; // 30 дней

        /// <summary>
        /// Имя claim для TenantId
        /// </summary>
        public const string TenantIdClaimType = "tenantId";

        /// <summary>
        /// Имя claim для TelegramId
        /// </summary>
        public const string TelegramIdClaimType = "telegramId";
    }

    /// <summary>
    /// Константы для демо-данных
    /// </summary>
    public static class Demo
    {
        /// <summary>
        /// Количество демо-треков для нового клиента
        /// </summary>
        public const int TracksCount = 3;
    }

    /// <summary>
    /// Константы для пагинации
    /// </summary>
    public static class Pagination
    {
        /// <summary>
        /// Размер страницы по умолчанию
        /// </summary>
        public const int DefaultPageSize = 20;

        /// <summary>
        /// Максимальный размер страницы
        /// </summary>
        public const int MaxPageSize = 100;
    }
}
