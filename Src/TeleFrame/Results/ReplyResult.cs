using TeleFrame.Common;
using TeleFrame.Results.Abstractions;

namespace TeleFrame.Results;

public class ReplyResult(
    string message,
    MessageId? messageId = null,
    ChatId? chatId = null,
    string? messageEffect = null,
    CancellationToken ct = default) : MessageResult(ct)
{
    protected override SendMessageRequest GenerateRequest(UpdateContext ctx)
    {
        return new SendMessageRequest
        {
            ChatId = chatId ?? ctx.Update.ResolveChatId(),

            ReplyParameters = new ReplyParameters
            {
                MessageId = messageId ?? ctx.Update.Message?.MessageId ??
                    throw new NullReferenceException("Message id is null"),
                AllowSendingWithoutReply = true
            },

            MessageEffectId = messageEffect,
            Text = message
        };
    }
}