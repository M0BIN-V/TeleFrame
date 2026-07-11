namespace TeleFrame.Results;

public static class Results
{
    public static TextResult Text(
        string message,
        ChatId? chatId = null,
        string? messageEffect = null,
        CancellationToken ct = default)
    {
        return new TextResult(message, chatId, messageEffect, ct);
    }

    public static ReplyResult Reply(
        string message,
        MessageId? messageId = null,
        ChatId? chatId = null,
        string? messageEffect = null,
        CancellationToken ct = default)
    {
        return new ReplyResult(message, messageId, chatId, messageEffect, ct);
    }
}