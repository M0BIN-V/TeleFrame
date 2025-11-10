using Microsoft.Extensions.DependencyInjection;
using TeleFrame.Middlewares.Abstractions;

namespace TeleFrame.Application;

internal class MiddlewareRegistry
{
    readonly UpdatePipelineBuilder _builder = new();

    public void Add(UpdateMiddleware middleware)
    {
        _builder.Add(middleware);
    }

    public void Add<T>(IServiceProvider services) where T : IUpdateMiddleware
    {
        _builder.Add(next => (ctx, ct) =>
        {
            var middleware = services.GetRequiredService<T>();
            return middleware.InvokeAsync(ctx, next, ct);
        });
    }

    public UpdateMiddlewareDelegate Build()
    {
        return _builder.Build();
    }
}