using Microsoft.Extensions.DependencyInjection;
using TeleFrame.Services;

namespace TeleFrame.UpdateHandlers;

public delegate Task UpdateHandlerFilterDelegate(UpdateContext context, CancellationToken ct);

public delegate UpdateHandlerFilterDelegate CommandHandlerFilter(UpdateHandlerFilterDelegate next);

public delegate Task UpdateHandlerDelegate(UpdateContext context, CancellationToken ct);

public class UpdateHandlerBuilder
{
    readonly List<CommandHandlerFilter> _filters = [];
    readonly UpdateHandlerDelegate _handler;

    readonly Lazy<UpdateHandlerDelegate> _lazyPipeline;

    public UpdateHandlerBuilder(UpdateHandlerDelegate handler)
    {
        _handler = handler;

        _lazyPipeline = new Lazy<UpdateHandlerDelegate>(BuildPipeline, true);
    }

    public UpdateHandlerBuilder Filter<T>() where T : IUpdateHandlerFilter
    {
        Filter(next => (ctx, ct) =>
        {
            var middleware = ctx.Services.GetRequiredService<T>();
            return middleware.InvokeAsync(ctx, next, ct);
        });

        return this;
    }

    public UpdateHandlerBuilder Filter(CommandHandlerFilter filter)
    {
        _filters.Add(filter);
        return this;
    }

    UpdateHandlerDelegate BuildPipeline()
    {
        UpdateHandlerFilterDelegate pipeline = (c, token) => _handler(c, token);

        for (var i = _filters.Count - 1; i >= 0; i--)
        {
            var next = pipeline;
            var filter = _filters[i];
            pipeline = filter(next);
        }

        return (ctx, ct) => pipeline(ctx, ct);
    }

    public UpdateHandlerDelegate Build()
    {
        return _lazyPipeline.Value;
    }
}