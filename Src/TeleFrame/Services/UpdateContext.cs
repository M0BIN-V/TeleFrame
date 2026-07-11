namespace TeleFrame.Services;

/// <summary>
/// Encapsulates all update-specific HTTP-like context information about an individual inbound Telegram update request.
/// </summary>
/// <remarks>
/// This class acts as the central state container passed down through the middleware pipeline, 
/// providing scoped access to the dependency injection engine and the active Telegram bot client instance.
/// </remarks>
/// <param name="serviceProvider">The scoped service provider context used for resolving nested dependencies during the update lifecycle.</param>
/// <param name="botClient">The underlying Telegram bot client instance responsible for API interactions.</param>
public class UpdateContext(IServiceProvider serviceProvider, ITelegramBotClient botClient)
{
    /// <summary>
    /// Gets the active Telegram bot client instance bound to this execution context.
    /// </summary>
    /// <value>The <see cref="ITelegramBotClient"/> interface used to invoke Telegram API methods.</value>
    public ITelegramBotClient Client { get; } = botClient;

    /// <summary>
    /// Gets the scoped service provider instance allocated specifically for processing the current update thread.
    /// </summary>
    /// <value>The <see cref="IServiceProvider"/> used to handle scoped dependency resolution inside handlers or middlewares.</value>
    public IServiceProvider Services { get; } = serviceProvider;

    /// <summary>
    /// Gets or sets the raw, deserialized incoming update payload dispatched from the Telegram API servers.
    /// </summary>
    /// <value>The concrete <see cref="Telegram.Bot.Types.Update"/> packet containing messages, queries, or event metadata.</value>
    public Update Update { get; set; } = null!;
}