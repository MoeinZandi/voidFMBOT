
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

        var chatId = message.Chat.Id;

        switch (command)
        {
            // START
            case "/start":
            case "/شروع":
            case "/shoroo":

                await _bot.SendMessage(
                    chatId: chatId,
                    text:
                        "🌑 به VOIDFM خوش اومدی!\n\n" +
                        "🎧 ایستگاه موسیقی گروه DEAD NIGHT\n\n" +
                        "برای دیدن دستورات ربات از /help استفاده کن.\n\n" +
                        "فارسی، انگلیسی و فینگلیش پشتیبانی می‌شن.",
                    cancellationToken: cancellationToken
                );

                break;

            // HELP
            case "/help":
            case "/راهنما":
            case "/rahnama":

                await _bot.SendMessage(
                    chatId: chatId,
                    text:
                        "🌑 VOIDFM — ربات موزیک DEAD NIGHT\n\n" +
                        "🎵 دستورات موسیقی:\n\n" +
                        "/play — پخش موسیقی\n" +
                        "/pakhsh — پخش موسیقی\n" +
                        "/پخش — پخش موسیقی\n\n" +
                        "/pause — مکث\n" +
                        "/maks — مکث\n" +
                        "/مکث — مکث\n\n" +
                        "/resume — ادامه پخش\n" +
                        "/edame — ادامه پخش\n" +
                        "/ادامه — ادامه پخش\n\n" +
                        "/skip — آهنگ بعدی\n" +
                        "/badi — آهنگ بعدی\n" +
                        "/بعدی — آهنگ بعدی\n\n" +
                        "/stop — توقف پخش\n" +
                        "/tavaqof — توقف پخش\n" +
                        "/توقف — توقف پخش\n\n" +
                        "/queue — نمایش صف موسیقی\n" +
                        "/saf — نمایش صف موسیقی\n" +
                        "/صف — نمایش صف موسیقی\n\n" +
                        "/leave — خروج از ویس چت\n" +
                        "/khorooj — خروج از ویس چت\n" +
                        "/خروج — خروج از ویس چت\n\n" +
                        "برای درخواست آهنگ:\n" +
                        "/play اسم آهنگ یا نام خواننده",
                    cancellationToken: cancellationToken
                );

                break;

            // PLAY
            case "/play":
            case "/پخش":
            case "/pakhsh":

                await _bot.SendMessage(
                    chatId: chatId,
                    text:
                        "🎵 درخواست پخش موسیقی دریافت شد!\n\n" +
                        "⚙️ موتور پخش و اتصال به ویس چت هنوز در حال راه‌اندازی است.\n" +
                        "به‌زودی می‌تونی آهنگ، خواننده، لینک یا فایل موسیقی بفرستی.",
                    cancellationToken: cancellationToken
                );

                break;

            // OTHER MUSIC COMMANDS
            case "/pause":
            case "/maks":
            case "/مکث":
            case "/resume":
            case "/edame":
            case "/ادامه":
            case "/skip":
            case "/badi":
            case "/بعدی":
            case "/stop":
            case "/tavaqof":
            case "/توقف":
            case "/queue":
            case "/saf":
            case "/صف":
            case "/leave":
            case "/khorooj":
            case "/خروج":

                await _bot.SendMessage(
                    chatId: chatId,
                    text:
                        "⏳ این قابلیت هنوز به موتور پخش موسیقی متصل نشده.\n" +
                        "در حال آماده‌سازی VOIDFM هستیم!",
                    cancellationToken: cancellationToken
                );

                break;
        }

        _logger.LogInformation(
            "Processed command {Command} in chat {ChatId}",
            command,
            chatId
        );
    }
}
