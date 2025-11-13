namespace TeleFrame.Results;

public class ReplyResult(
    string message,
    MessageId? messageId = null,
    ChatId? chatId = null,
    CancellationToken ct = default) : MessageResult(ct)
{
    protected override SendMessageRequest GenerateRequest(UpdateContext ctx)
    {
        return new SendMessageRequest
        {
            ChatId = chatId ?? ctx.Update.Message?.Chat ?? throw new NullReferenceException("Chat id is null"),
            ReplyParameters = new ReplyParameters
            {
                MessageId = messageId ?? ctx.Update.Message?.MessageId ??
                    throw new NullReferenceException("Message id is null"),
                AllowSendingWithoutReply = true
            },
            Text = message
        };
    }
}