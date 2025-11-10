using Microsoft.Extensions.DependencyInjection;
using TeleFrame.Middlewares.Abstractions;
using TeleFrame.Services;

namespace TeleFrame.Application;

internal class UpdateProcessor(
    IServiceProvider services,
    UpdateMiddlewareDelegate pipeline)
{
    public async Task ProcessAsync(IAsyncEnumerable<Update> updates, CancellationToken ct)
    {
        var logger = services.GetRequiredService<ILogger<UpdateProcessor>>();
        
        await foreach (var update in updates.WithCancellation(ct))
            try
            {
                using var scope = services.CreateScope();
                var ctx = scope.ServiceProvider.GetRequiredService<UpdateContext>();
                ctx.Update = update;

                await pipeline(ctx, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Pipeline processing error");
            }
    }
}