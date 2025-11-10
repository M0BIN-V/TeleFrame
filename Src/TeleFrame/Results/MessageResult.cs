using TeleFrame.Services;
using Telegram.Bot.Requests;

namespace TeleFrame.Results;

public abstract class MessageResult(CancellationToken ct = default) : ITelegramResult
{
    public virtual Task InvokeAsync(UpdateContext ctx)
    {
        return ctx.Client.SendRequest(GenerateRequest(ctx), ct);
    }

    protected abstract SendMessageRequest GenerateRequest(UpdateContext ctx);
}