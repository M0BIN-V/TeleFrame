namespace TeleFrame.Middlewares.Abstractions;

public interface IUpdateMiddleware
{
    Task InvokeAsync(UpdateContext context, UpdateMiddlewareDelegate next, CancellationToken cancellationToken);
}