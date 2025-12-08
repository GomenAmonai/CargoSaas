using Cargo.Core;
using Cargo.Core.Entities;
using Cargo.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Cargo.Infrastructure.Services;

/// <summary>
/// Фоновый сервис для обработки Telegram Bot обновлений через Long Polling
/// </summary>
public class TelegramBotBackgroundService : IHostedService
{
    private readonly ILogger<TelegramBotBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private ITelegramBotClient? _botClient;
    private CancellationTokenSource? _cancellationTokenSource;

    public TelegramBotBackgroundService(
        ILogger<TelegramBotBackgroundService> logger,
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var botToken = _configuration["Telegram:BotToken"];
        
        if (string.IsNullOrEmpty(botToken))
        {
            _logger.LogWarning("Telegram:BotToken not configured. Bot will not start.");
            return Task.CompletedTask;
        }

        var webAppUrl = _configuration["Telegram:WebAppUrl"];
        
        if (string.IsNullOrEmpty(webAppUrl))
        {
            _logger.LogWarning("Telegram:WebAppUrl not configured. Using placeholder.");
            webAppUrl = "https://your-app-url.com"; // Placeholder
        }

        _logger.LogInformation("Starting Telegram Bot with Long Polling...");

        _botClient = new TelegramBotClient(botToken);
        _cancellationTokenSource = new CancellationTokenSource();

        // Настройки приема обновлений
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message }, // Принимаем только сообщения
            ThrowPendingUpdates = true // Игнорировать старые обновления при запуске
        };

        // Запускаем Long Polling
        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cancellationTokenSource.Token
        );

        _logger.LogInformation("Telegram Bot started successfully!");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Telegram Bot...");

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();

        _logger.LogInformation("Telegram Bot stopped.");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Обработчик входящих обновлений от Telegram
    /// </summary>
    private async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        try
        {
            // Обрабатываем только текстовые сообщения
            if (update.Message is not { Text: { } messageText } message)
                return;

            var chatId = message.Chat.Id;
            var username = message.From?.Username ?? "Unknown";

            _logger.LogInformation("Received message from @{Username}: {MessageText}", username, messageText);

            // Обработка команды /start
            if (messageText.Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                await HandleStartCommandAsync(botClient, chatId, cancellationToken);
            }
            // Обработка команды /createManager
            else if (messageText.StartsWith("/createManager", StringComparison.OrdinalIgnoreCase))
            {
                await HandleCreateManagerCommandAsync(botClient, message, cancellationToken);
            }
            // Обработка команды /removeManager (вернуть роль Client)
            else if (messageText.StartsWith("/removeManager", StringComparison.OrdinalIgnoreCase))
            {
                await HandleRemoveManagerCommandAsync(botClient, message, cancellationToken);
            }
            else
            {
                // Для всех остальных сообщений отправляем инструкцию
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Available commands:\n" +
                          "/start - Open the app\n" +
                          "/createManager <telegramId> - Make user a Manager (Admin only)\n" +
                          "/removeManager <telegramId> - Remove Manager role, return to Client (Admin only)",
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update");
        }
    }

    /// <summary>
    /// Обработчик команды /start
    /// </summary>
    private async Task HandleStartCommandAsync(
        ITelegramBotClient botClient,
        long chatId,
        CancellationToken cancellationToken)
    {
        var webAppUrl = _configuration["Telegram:WebAppUrl"] ?? "https://your-app-url.com";

        // Создаем inline keyboard с кнопкой для открытия WebApp
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithWebApp(
                    text: "🚀 Open App",
                    webAppInfo: new WebAppInfo { Url = webAppUrl }
                )
            }
        });

        var welcomeMessage = 
            "🎉 *Welcome to Cargo System!*\n\n" +
            "Track your packages easily and stay updated on their status.\n\n" +
            "Click the button below to get started:";

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: welcomeMessage,
            parseMode: ParseMode.Markdown,
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Sent welcome message to chat {ChatId}", chatId);
    }

    /// <summary>
    /// Обработчик команды /createManager <telegramId>
    /// Позволяет админу назначить роль Manager существующему пользователю
    /// </summary>
    private async Task HandleCreateManagerCommandAsync(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var adminTelegramId = message.From?.Id;
        var messageText = message.Text ?? string.Empty;

        // Парсим команду: /createManager <telegramId>
        var parts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length < 2)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "❌ Usage: /createManager <telegramId>\n\n" +
                      "Example: /createManager 123456789",
                cancellationToken: cancellationToken
            );
            return;
        }

        if (!long.TryParse(parts[1], out var targetTelegramId))
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "❌ Invalid Telegram ID format. Please provide a valid number.",
                cancellationToken: cancellationToken
            );
            return;
        }

        try
        {
            // Создаем scope для доступа к сервисам
            using var scope = _serviceScopeFactory.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            // Проверяем что отправитель - админ
            // 1. Проверяем по роли SystemAdmin в БД
            // 2. Или по списку админов из конфигурации (для первого запуска)
            var adminUser = await userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.TelegramId == adminTelegramId, cancellationToken);

            // Получаем список админов из конфигурации (Telegram IDs через запятую)
            var adminTelegramIds = _configuration["Admin:TelegramIds"]?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => long.TryParse(id.Trim(), out var parsed) ? parsed : (long?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList() ?? new List<long>();

            var isAdminByRole = adminUser?.Role == UserRole.SystemAdmin;
            
            // КРИТИЧНО: Проверяем что adminTelegramId не null перед проверкой в списке
            // Если null, то не проверяем в списке (избегаем проверки нуля)
            var isAdminByConfig = adminTelegramId.HasValue && adminTelegramIds.Contains(adminTelegramId.Value);

            if (!isAdminByRole && !isAdminByConfig)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "❌ Access denied. Only SystemAdmin can use this command.\n\n" +
                          "To become SystemAdmin, set your Telegram ID in Railway environment variables:\n" +
                          "`Admin__TelegramIds=your_telegram_id`",
                    cancellationToken: cancellationToken
                );
                _logger.LogWarning("Unauthorized attempt to use /createManager by TelegramId: {TelegramId}", adminTelegramId);
                return;
            }

            // Ищем пользователя по TelegramId
            var targetUser = await userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.TelegramId == targetTelegramId, cancellationToken);

            if (targetUser == null)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"❌ User with Telegram ID {targetTelegramId} not found in database.\n\n" +
                          "User must first log in through the app to be created.",
                    cancellationToken: cancellationToken
                );
                return;
            }

            // Проверяем текущую роль
            if (targetUser.Role == UserRole.Manager)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"ℹ️ User {targetTelegramId} is already a Manager.",
                    cancellationToken: cancellationToken
                );
                return;
            }

            // Назначаем роль Manager
            targetUser.Role = UserRole.Manager;
            targetUser.UpdatedAt = DateTime.UtcNow;

            var result = await userManager.UpdateAsync(targetUser);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "User {TelegramId} promoted to Manager by admin {AdminTelegramId}",
                    targetTelegramId,
                    adminTelegramId
                );

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"✅ Success! User {targetTelegramId} is now a Manager.\n\n" +
                          $"Name: {targetUser.FirstName} {targetUser.LastName}\n" +
                          $"Username: @{targetUser.UserName}\n" +
                          $"Role: {targetUser.Role}",
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to update user role: {Errors}", errors);
                
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"❌ Failed to update user role: {errors}",
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing /createManager command");
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "❌ An error occurred while processing the command. Please try again later.",
                cancellationToken: cancellationToken
            );
        }
    }

    /// <summary>
    /// Обработчик команды /removeManager <telegramId>
    /// Убирает роль Manager и возвращает пользователя к роли Client
    /// </summary>
    private async Task HandleRemoveManagerCommandAsync(
        ITelegramBotClient botClient,
        Telegram.Bot.Types.Message message,
        CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var adminTelegramId = message.From?.Id;

        var messageText = message.Text ?? string.Empty;

        // Парсим команду: /removeManager <telegramId>
        var parts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length < 2)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "❌ Usage: /removeManager <telegramId>\n\n" +
                      "Example: /removeManager 123456789",
                cancellationToken: cancellationToken
            );
            return;
        }

        if (!long.TryParse(parts[1], out var targetTelegramId))
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "❌ Invalid Telegram ID format. Please provide a valid number.",
                cancellationToken: cancellationToken
            );
            return;
        }

        try
        {
            // Создаем scope для доступа к сервисам
            using var scope = _serviceScopeFactory.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            // Проверяем что отправитель - админ (та же логика что и для /createManager)
            var adminUser = await userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.TelegramId == adminTelegramId, cancellationToken);

            var adminTelegramIds = _configuration["Admin:TelegramIds"]?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => long.TryParse(id.Trim(), out var parsed) ? parsed : (long?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList() ?? new List<long>();

            var isAdminByRole = adminUser?.Role == UserRole.SystemAdmin;
            var isAdminByConfig = adminTelegramId.HasValue && adminTelegramIds.Contains(adminTelegramId.Value);

            if (!isAdminByRole && !isAdminByConfig)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "❌ Access denied. Only SystemAdmin can use this command.",
                    cancellationToken: cancellationToken
                );
                _logger.LogWarning("Unauthorized attempt to use /removeManager by TelegramId: {TelegramId}", adminTelegramId);
                return;
            }

            // Ищем пользователя по TelegramId
            var targetUser = await userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.TelegramId == targetTelegramId, cancellationToken);

            if (targetUser == null)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"❌ User with Telegram ID {targetTelegramId} not found in database.",
                    cancellationToken: cancellationToken
                );
                return;
            }

            // Проверяем текущую роль
            if (targetUser.Role != UserRole.Manager)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"ℹ️ User {targetTelegramId} is not a Manager. Current role: {targetUser.Role}",
                    cancellationToken: cancellationToken
                );
                return;
            }

            // Возвращаем роль Client
            targetUser.Role = UserRole.Client;
            targetUser.UpdatedAt = DateTime.UtcNow;
            var result = await userManager.UpdateAsync(targetUser);

            if (!result.Succeeded)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"❌ Failed to update user role. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}",
                    cancellationToken: cancellationToken
                );
                _logger.LogError("Failed to remove Manager role for user {TelegramId}: {Errors}", 
                    targetTelegramId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"✅ Success! User {targetTelegramId} is now a Client again.",
                cancellationToken: cancellationToken
            );

            _logger.LogInformation(
                "User {TelegramId} role changed from Manager to Client by admin {AdminTelegramId}",
                targetTelegramId, adminTelegramId);
        }
        catch (Exception ex)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "❌ An error occurred while processing the command. Please try again later.",
                cancellationToken: cancellationToken
            );
            _logger.LogError(ex, "Error processing /removeManager command");
        }
    }

    /// <summary>
    /// Обработчик ошибок polling
    /// </summary>
    private Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Telegram Bot polling error occurred");
        return Task.CompletedTask;
    }
}

