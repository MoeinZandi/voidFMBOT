
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using TelegramMusicBot.Bot;

// First, read the token from the hosting environment.
var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

// If no environment variable exists, try loading the local .env file.
if (string.IsNullOrWhiteSpace(token))
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

    while (directory is not null &&
           !File.Exists(Path.Combine(directory.FullName, ".env")))
    {
        directory = directory.Parent;
    }

    if (directory is not null)
    {
        Env.Load(Path.Combine(directory.FullName, ".env"));
        token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
    }
}

if (string.IsNullOrWhiteSpace(token))
{
    throw new InvalidOperationException(
        "TELEGRAM_BOT_TOKEN is missing. Configure it as an environment variable."
    );
}

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ITelegramBotClient>(
    _ => new TelegramBotClient(token)
);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.RunAsync();
