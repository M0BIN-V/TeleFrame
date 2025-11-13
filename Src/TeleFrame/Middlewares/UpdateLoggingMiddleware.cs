using TeleFrame.Middlewares.Abstractions;

namespace TeleFrame.Middlewares;

public partial class UpdateLoggingMiddleware(ILogger<UpdateLoggingMiddleware> logger) : IUpdateMiddleware
{
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

    [LoggerMessage(LogLevel.Information, "Received Update Type : {Type}")]
    static partial void LogReceivedUpdateType(ILogger<UpdateLoggingMiddleware> logger, UpdateType type);

    [LoggerMessage(LogLevel.Information, "Received Message : {Message}")]
    static partial void LogReceivedMessageMessage(ILogger<UpdateLoggingMiddleware> logger, string message);

    [LoggerMessage(LogLevel.Information, "Received Message Type: {Message}")]
    static partial void LogReceivedMessageTypeMessage(ILogger<UpdateLoggingMiddleware> logger, MessageType message);
}

public static class UpdateLoggingMiddlewareExtensions
{
    public static IServiceCollection AddUpdateLogging(this IServiceCollection service)
    {
        service.AddSingleton<UpdateLoggingMiddleware>();
        return service;
    }

    public static TelegramBotApplication UseUpdateLogging(this TelegramBotApplication app)
    {
        app.Use<UpdateLoggingMiddleware>();
        return app;
    }
}