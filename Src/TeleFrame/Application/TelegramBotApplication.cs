using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TeleFrame.Middlewares.Abstractions;
using Telegram.Bot.Polling;

namespace TeleFrame.Application;

public partial class TelegramBotApplication : IHost
{
    readonly ITelegramBotClient _client;
    readonly IHost _host;
    readonly ILogger<TelegramBotApplication> _logger;
    readonly MiddlewareRegistry _middlewareRegistry = new();
    readonly ReceiverOptions _receiverOptions;
    readonly UpdateChannel _updateChannel = new();
    readonly ProcessingOptions _processingOptions;

    public TelegramBotApplication(
        IHost host, ReceiverOptions receiverOptions,
        ProcessingOptions processingOptions)
    {
        _receiverOptions = receiverOptions;
        _processingOptions = processingOptions;
        _host = host;
        _logger = _host.Services.GetRequiredService<ILogger<TelegramBotApplication>>();
        _client = _host.Services.GetRequiredService<ITelegramBotClient>();
    }

    public IConfiguration Configuration => _host.Services.GetRequiredService<IConfiguration>();
    public IServiceProvider Services => _host.Services;

    public void Dispose()
    {
        _host.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task StartAsync(CancellationToken cancellationToken = new())
    {
        LogTelegramBotStarting();

        var pipeline = _middlewareRegistry.Build();
        var processor = new UpdateProcessor(Services, pipeline);

        for (var i = 0; i < _processingOptions.WorkerCount; i++)
            _ = Task.Run(() => processor.ProcessAsync(_updateChannel.Reader, cancellationToken), cancellationToken);

        _client.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            _receiverOptions,
            cancellationToken
        );

        var me = await _client.GetMe(cancellationToken);
        
        _logger.LogInformation("Bot started receiving from {UserName}", me.Username);

        await _host.WaitForShutdownAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = new())
    {
        _logger.LogInformation("Telegram bot stopping...");
        return Task.CompletedTask;
    }

    public TelegramBotApplication Use(UpdateMiddleware middleware)
    {
        _middlewareRegistry.Add(middleware);
        return this;
    }

    public TelegramBotApplication Use<T>() where T : IUpdateMiddleware
    {
        _middlewareRegistry.Add<T>(Services);
        return this;
    }

    async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken ct)
    {
        await _updateChannel.Writer.WriteAsync(update, ct);
    }

    Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        _logger.LogError(ex, "Telegram error");
        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Information, "Telegram bot starting...")]
    partial void LogTelegramBotStarting();
}