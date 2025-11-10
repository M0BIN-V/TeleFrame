using TeleFrame.Middlewares.Abstractions;

namespace TeleFrame.Application;

internal class UpdatePipelineBuilder
{
    readonly List<UpdateMiddleware> _middlewares = new();

    public void Add(UpdateMiddleware middleware)
    {
        _middlewares.Add(middleware);
    }

    public UpdateMiddlewareDelegate Build()
    {
        return _middlewares
            .AsEnumerable()
            .Reverse()
            .Aggregate(
                (UpdateMiddlewareDelegate)((_, _) => Task.CompletedTask),
                (next, middleware) => middleware(next)
            );
    }
}