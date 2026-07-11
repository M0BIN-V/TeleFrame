using TeleFrame.Common;
using TeleFrame.Results.Abstractions;

namespace TeleFrame.Results;

public class TextResult(
    string message,
    ChatId? chatId = null,
    string? messageEffect = null,
    CancellationToken ct = default) : MessageResult(ct)
{
    protected override SendMessageRequest GenerateRequest(UpdateContext ctx)
    {
        return new SendMessageRequest
        {
            ChatId = chatId ?? ctx.Update.ResolveChatId(),
            Text = message,
            MessageEffectId = messageEffect
        };
    }
}