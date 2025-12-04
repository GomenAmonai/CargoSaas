using Microsoft.Extensions.Configuration;
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
    private ITelegramBotClient? _botClient;
    private CancellationTokenSource? _cancellationTokenSource;

    public TelegramBotBackgroundService(
        ILogger<TelegramBotBackgroundService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
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
            else
            {
                // Для всех остальных сообщений отправляем инструкцию
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Please use /start to begin or click the button to open the app.",
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

