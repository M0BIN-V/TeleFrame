using TeleFrame.Middlewares.Abstractions;

namespace Sample;

public class BlackListMiddleware : IUpdateMiddleware
{
    public Task InvokeAsync(UpdateContext context, UpdateMiddlewareDelegate next, CancellationToken cancellationToken)
    {
        var blackList = new List<long> { 123456789, 987654321 }; // Example blacklisted user IDs

        if (context.Update.Message?.From != null && blackList.Contains(context.Update.Message.From.Id))
            // Ignore the update for blacklisted users
            return Task.CompletedTask;

        return next(context, cancellationToken);
    }
}