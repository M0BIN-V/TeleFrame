using TeleFrame.Middlewares.Abstractions;

namespace TeleFrame.Middlewares;

/// <summary>
/// A middleware component responsible for intercepting and logging information about incoming Telegram updates.
/// </summary>
/// <remarks>
/// This middleware utilizes High-Performance Source Generated logging (<see cref="LoggerMessageAttribute"/>) 
/// to minimize allocations and maximize performance when processing heavy streams of update objects.
/// </remarks>
public partial class UpdateLoggingMiddleware(ILogger<UpdateLoggingMiddleware> logger) : IUpdateMiddleware
{
    /// <summary>
    /// Executes the logging operation for the incoming update context and passes control to the next middleware in the execution pipeline.
    /// </summary>
    /// <param name="context">The state context representing the current inbound Telegram update payload.</param>
    /// <param name="next">The delegate representing the next structural step in the middleware processing pipeline.</param>
    /// <param name="cancellationToken">A token used to monitor for operation cancellations.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous pipeline execution chain.</returns>
    public Task InvokeAsync(UpdateContext context, UpdateMiddlewareDelegate next,
        CancellationToken cancellationToken)
    {
        var update = context.Update;

        LogReceivedUpdateType(logger, update.Type);

        if (update.Message?.Text is null) return next(context, cancellationToken);

        LogReceivedMessageMessage(logger, update.Message.Text);
        LogReceivedMessageTypeMessage(logger, update.Message.Type);

        return next(context, cancellationToken);
    }

    /// <summary>
    /// Emits a source-generated high-performance log event containing the top-level Telegram update type identifier.
    /// </summary>
    /// <param name="logger">The designated <see cref="ILogger"/> instance utilized to emit logs.</param>
    /// <param name="type">The incoming raw type categorization of the Telegram update.</param>
    [LoggerMessage(LogLevel.Information, "Received Update Type : {Type}")]
    static partial void LogReceivedUpdateType(ILogger<UpdateLoggingMiddleware> logger, UpdateType type);

    /// <summary>
    /// Emits a source-generated high-performance log event displaying the core string text contents extracted from the message payload.
    /// </summary>
    /// <param name="logger">The designated <see cref="ILogger"/> instance utilized to emit logs.</param>
    /// <param name="message">The absolute text content forwarded by the user inside the chat context.</param>
    [LoggerMessage(LogLevel.Information, "Received Message : {Message}")]
    static partial void LogReceivedMessageMessage(ILogger<UpdateLoggingMiddleware> logger, string message);

    /// <summary>
    /// Emits a source-generated high-performance log event declaring the specific structural sub-type classification of the received message payload.
    /// </summary>
    /// <param name="logger">The designated <see cref="ILogger"/> instance utilized to emit logs.</param>
    /// <param name="message">The specific message variant type (e.g., Text, Photo, Video, Document, etc.).</param>
    [LoggerMessage(LogLevel.Information, "Received Message Type: {Message}")]
    static partial void LogReceivedMessageTypeMessage(ILogger<UpdateLoggingMiddleware> logger, MessageType message);
}

/// <summary>
/// Provides extension abstractions to configure and incorporate the logging middleware easily into the Dependency Injection 
/// service descriptor pool and the application middleware request pipeline.
/// </summary>
public static class UpdateLoggingMiddlewareExtensions
{
    /// <summary>
    /// Injects and registers the <see cref="UpdateLoggingMiddleware"/> into the application's central DI container infrastructure as a singleton dependency.
    /// </summary>
    /// <param name="service">The target service container collection descriptor collection.</param>
    /// <returns>The primary <see cref="IServiceCollection"/> descriptor root reference to facilitate smooth fluent method chaining.</returns>
    public static IServiceCollection AddUpdateLogging(this IServiceCollection service)
    {
        service.AddSingleton<UpdateLoggingMiddleware>();
        return service;
    }

    /// <summary>
    /// Appends the <see cref="UpdateLoggingMiddleware"/> block directly into the dynamic Telegram application pipeline flow sequence.
    /// </summary>
    /// <param name="app">The active Telegram bot builder runtime configuration application context container instance.</param>
    /// <returns>The current modified <see cref="TelegramBotApplication"/> instance pipeline structure framework to facilitate smooth fluent method chaining.</returns>
    public static TelegramBotApplication UseUpdateLogging(this TelegramBotApplication app)
    {
        app.Use<UpdateLoggingMiddleware>();
        return app;
    }
}