namespace Cargo.Core.Enums;

/// <summary>
/// Роли пользователей в системе
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Системный администратор (полный доступ ко всем тенантам)
    /// </summary>
    SystemAdmin = 0,

    /// <summary>
    /// Менеджер компании (управление грузами, доступ через email/password)
    /// </summary>
    Manager = 1,

    /// <summary>
    /// Клиент (просмотр своих грузов, доступ через Telegram)
    /// </summary>
    Client = 2
}

