
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using TelegramMusicBot.Bot;

// Find the .env file in the project root.
var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

while (directory is not null &&
       !File.Exists(Path.Combine(directory.FullName, ".env")))
{
    directory = directory.Parent;
}

if (directory is null)
{
    throw new FileNotFoundException(
        "Could not find the .env file in the project directory or its parent directories."
    );
}

Env.Load(Path.Combine(directory.FullName, ".env"));

var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

if (string.IsNullOrWhiteSpace(token))
{
    throw new InvalidOperationException(
        "TELEGRAM_BOT_TOKEN is missing from the .env file."
    );
}

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ITelegramBotClient>(
    _ => new TelegramBotClient(token)
);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.RunAsync();
