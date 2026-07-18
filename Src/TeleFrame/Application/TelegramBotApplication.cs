using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TeleFrame.Middlewares.Abstractions;
using Telegram.Bot.Polling;

namespace TeleFrame.Application;

/// <summary>
///     Represents the core host wrapper for managing the lifecycle, middleware pipeline,
///     and background processing of a Telegram Bot application.
/// </summary>
/// <remarks>
///     This class acts as a facade over an underlying <see cref="IHost" /> instance, coordinating
///     multi-worker background updates via internal channels and dynamic middleware execution pipelines.
/// </remarks>
public partial class TelegramBotApplication : IHost
{
    private readonly ITelegramBotClient _client;
    private readonly IHost _host;
    private readonly ILogger<TelegramBotApplication> _logger;
    private readonly MiddlewareRegistry _middlewareRegistry = new();
    private readonly ProcessingOptions _processingOptions;
    private readonly ReceiverOptions _receiverOptions;
    private readonly UpdateChannel _updateChannel = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="TelegramBotApplication" /> class.
    /// </summary>
    /// <param name="host">The underlying Generic Host providing infrastructure, logging, and dependency injection container.</param>
    /// <param name="receiverOptions">Configuration options specifying behavior for the Telegram long-polling receiver.</param>
    /// <param name="processingOptions">Configuration options managing worker counts and parallel processing behaviors.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if required services like <see cref="ILogger" /> or
    ///     <see cref="ITelegramBotClient" /> cannot be resolved from the host container.
    /// </exception>
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

    /// <summary>
    ///     Gets the application configuration root loaded from the underlying host context.
    /// </summary>
    /// <value>The <see cref="IConfiguration" /> instance containing configuration keys and values.</value>
    public IConfiguration Configuration => _host.Services.GetRequiredService<IConfiguration>();

    /// <summary>
    ///     Gets the centralized service provider containing all registered application dependencies.
    /// </summary>
    /// <value>The <see cref="IServiceProvider" /> used for dependency resolution.</value>
    public IServiceProvider Services => _host.Services;

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources,
    ///     specifically disposing the underlying Generic Host infrastructure.
    /// </summary>
    public void Dispose()
    {
        _host.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Asynchronously bootstraps the Telegram Bot application, spins up parallel background processing workers,
    ///     starts polling updates from Telegram API, and blocks execution until a shutdown signal is intercepted.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous startup and execution lifecycle of the host.</returns>
    /// <remarks>
    ///     Worker tasks are deliberately unawaited (<c>_ = Task.Run(...)</c>) to dynamically execute parallel consumption
    ///     from the <see cref="UpdateChannel" /> while keeping the main loop decoupled.
    /// </remarks>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _host.StartAsync(cancellationToken);

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

    /// <summary>
    ///     Triggers the stopping routine for the Telegram Bot application.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to monitor for cancellation requests during termination.</param>
    /// <returns>
    ///     A completed <see cref="Task" /> since the underlying host lifecycle is gracefully controlled via shutdown
    ///     signals.
    /// </returns>
    public Task StopAsync(CancellationToken cancellationToken = new())
    {
        _logger.LogInformation("Telegram bot stopping...");
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Registers a raw custom inline update middleware delegate into the execution pipeline.
    /// </summary>
    /// <param name="middleware">The middleware delegate defining the operational step for incoming updates.</param>
    /// <returns>The current <see cref="TelegramBotApplication" /> instance to support fluent method chaining configurations.</returns>
    public TelegramBotApplication Use(UpdateMiddleware middleware)
    {
        _middlewareRegistry.Add(middleware);
        return this;
    }

    /// <summary>
    ///     Registers a strongly-typed middleware component into the execution pipeline.
    ///     The target middleware must be predefined and registered inside the DI container.
    /// </summary>
    /// <typeparam name="T">The type of the middleware component extending <see cref="IUpdateMiddleware" />.</typeparam>
    /// <returns>The current <see cref="TelegramBotApplication" /> instance to support fluent method chaining configurations.</returns>
    /// <remarks>
    ///     Enforce that dependencies needed by <typeparamref name="T" /> are properly declared inside the application's
    ///     ServiceCollection beforehand.
    /// </remarks>
    public TelegramBotApplication Use<T>() where T : IUpdateMiddleware
    {
        _middlewareRegistry.Add<T>(Services);
        return this;
    }

    /// <summary>
    ///     Acts as the target polling callback, asynchronously piping incoming Telegram updates into the internal producer
    ///     channel.
    /// </summary>
    /// <param name="client">The active Telegram bot client instance dispatching the payload.</param>
    /// <param name="update">The deserialized incoming update payload containing messages, queries, or actions from Telegram.</param>
    /// <param name="ct">A token that monitors for request timeouts or process cancellation.</param>
    /// <returns>A <see cref="Task" /> representing the completion of the channel write operation.</returns>
    private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken ct)
    {
        await _updateChannel.Writer.WriteAsync(update, ct);
    }

    /// <summary>
    ///     Handles exceptions thrown during the polling loop operations of the Telegram Bot client.
    /// </summary>
    /// <param name="bot">The active Telegram bot client instance that encountered the exception.</param>
    /// <param name="ex">The raw exception containing error details from network layers or API constraints.</param>
    /// <param name="ct">A token that monitors for cancellation requests.</param>
    /// <returns>A completed <see cref="Task" /> indicating that the error has been caught and logged.</returns>
    private Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        _logger.LogError(ex, "Telegram error");
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Emits a source-generated high-performance log event indicating that the Telegram bot initialization process has
    ///     commenced.
    /// </summary>
    [LoggerMessage(LogLevel.Information, "Telegram bot starting...")]
    partial void LogTelegramBotStarting();
}