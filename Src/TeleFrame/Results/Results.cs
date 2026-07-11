namespace TeleFrame.Results;

/// <summary>
/// Provides a centralized static factory for constructing standardized action results used to dispatch responses back to Telegram.
/// </summary>
/// <remarks>
/// This pattern mirrors the Minimal APIs <c>Results</c> design paradigm in ASP.NET Core, 
/// abstracting API calls into fluent, decoupled execution results.
/// </remarks>
public static class Results
{
    /// <summary>
    /// Creates a standard text message result targeted for a specific chat or the current active conversation.
    /// </summary>
    /// <param name="message">The text contents to be delivered to the recipient.</param>
    /// <param name="chatId">The destination chat identifier. If omitted, the context processor typically falls back to the active user's chat.</param>
    /// <param name="messageEffect">The optional unique identifier for a premium custom message animation effect.</param>
    /// <param name="ct">A token to monitor for request cancellation or timeouts.</param>
    /// <returns>A configured <see cref="TextResult"/> object ready to be dispatched by the execution engine.</returns>
    public static TextResult Text(
        string message,
        ChatId? chatId = null,
        string? messageEffect = null,
        CancellationToken ct = default)
    {
        return new TextResult(message, chatId, messageEffect, ct);
    }

    /// <summary>
    /// Creates a reply message result linked structurally to a previous incoming message ID.
    /// </summary>
    /// <param name="message">The text contents to be delivered as a reply.</param>
    /// <param name="messageId">The specific targeting message identifier being replied to. If null, the dispatcher usually infers the current message.</param>
    /// <param name="chatId">The destination chat identifier. If omitted, defaults to the current execution thread context.</param>
    /// <param name="messageEffect">The optional unique identifier for a premium custom message animation effect.</param>
    /// <param name="ct">A token to monitor for request cancellation or timeouts.</param>
    /// <returns>A configured <see cref="ReplyResult"/> object designed to trigger a conversational reply link.</returns>
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