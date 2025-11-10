using Microsoft.Extensions.DependencyInjection;
using TeleFrame.Application;
using TeleFrame.Middlewares.Abstractions;
using TeleFrame.Services;

namespace TeleFrame.Middlewares;

public class UpdateLoggingMiddleware(ILogger<UpdateLoggingMiddleware> logger) : IUpdateMiddleware
{
    public Task InvokeAsync(UpdateContext context, UpdateMiddlewareDelegate next,
        CancellationToken cancellationToken)
    {
        var update = context.Update;

        logger.LogInformation("Received Update Type : {Type}", update.Type);
        if (update.Message?.Text is not null)
            logger.LogInformation("Received Message : {Message}", update.Message.Text);
        return next(context, cancellationToken);
    }
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