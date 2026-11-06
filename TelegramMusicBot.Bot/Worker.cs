
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramMusicBot.Bot;

public sealed class Worker : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly ILogger<Worker> _logger;

    public Worker(
        ITelegramBotClient bot,
        ILogger<Worker> logger)
    {
        _bot = bot;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var me = await _bot.GetMe(
            cancellationToken: stoppingToken
        );

        _logger.LogInformation(
            "VOIDFM is online! Bot username: @{Username}",
            me.Username
        );

        int? offset = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var updates = await _bot.GetUpdates(
                    offset: offset,
                    timeout: 30,
                    allowedUpdates: [UpdateType.Message],
                    cancellationToken: stoppingToken
                );

                foreach (var update in updates)
                {
                    offset = update.Id + 1;

                    if (update.Message is not { } message)
                        continue;

                    if (string.IsNullOrWhiteSpace(message.Text))
                        continue;

                    await HandleMessageAsync(
                        message,
                        stoppingToken
                    );
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (ApiRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Telegram API error: {Message}",
                    ex.Message
                );

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while receiving Telegram updates."
                );

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken
                );
            }
        }
    }

    private async Task HandleMessageAsync(
        Message message,
        CancellationToken cancellationToken)
    {
        var text = message.Text!.Trim();

        var command = text
            .Split(' ', 2)[0]
            .Split('@')[0]
            .ToLowerInvariant();

        switch (command)
        {
            case "/start":
                await _bot.SendMessage(
                    chatId: message.Chat.Id,
                    text:
                        "🌑 Welcome to VOIDFM!\n\n" +
                        "Your group's underground music station.\n\n" +
                        "Use /help to see available commands.",
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation(
                    "Handled /start in chat {ChatId}",
                    message.Chat.Id
                );

                break;

            case "/help":
                await _bot.SendMessage(
                    chatId: message.Chat.Id,
                    text:
                        "🎧 VOIDFM — Music Bot\n\n" +
                        "Available commands:\n\n" +
                        "/start - Start the bot\n" +
                        "/help - Show this help message\n" +
                        "/play - Play music (coming soon)\n" +
                        "/search - Search for music (coming soon)\n" +
                        "/queue - Show music queue (coming soon)\n" +
                        "/skip - Skip current track (coming soon)\n" +
                        "/stop - Stop playback (coming soon)",
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation(
                    "Handled /help in chat {ChatId}",
                    message.Chat.Id
                );

                break;
        }
    }
}
