namespace TeleFrame.Results;

public class TextResult(string message, ChatId? chatId = null, CancellationToken ct = default) : MessageResult(ct)
{
    protected override SendMessageRequest GenerateRequest(UpdateContext ctx)
    {
        return new SendMessageRequest
        {
            ChatId = chatId ?? ctx.Update.Message?.Chat ?? throw new NullReferenceException("Chat id is null"),
            Text = message
        };
    }
}