namespace TeleFrame.UpdateHandlers.MessageHandlers.CommandHandlers;

/// <summary>
/// Provides extension members utilizing modern C# extension syntax to fluently map 
/// command-based message handlers directly onto the <see cref="TelegramBotApplication"/> instance.
/// </summary>
public static class CommandHandlerExtensions
{
    extension(TelegramBotApplication app)
    {
        /// <summary>
        /// Maps an incoming text message command evaluating against a custom functional predicate 
        /// to a strongly-typed update handler delegate.
        /// </summary>
        /// <param name="predicate">The evaluation logic applied to the extracted text message content to verify if the command matches.</param>
        /// <param name="handler">The core delegate execution logic triggered when the command matches the criteria.</param>
        /// <returns>A <see cref="MessageHandlerBuilder"/> instance used to further configure or chain configurations for the mapped message pipeline.</returns>
        /// <remarks>
        /// This method automatically enforces pre-filtering to ensure that incoming updates are explicitly structured as 
        /// <see cref="MessageType.Text"/> variants prior to invoking the predicate.
        /// </remarks>
        public MessageHandlerBuilder MapCommand(Func<string, bool> predicate, UpdateHandlerDelegate handler)
        {
            return app.MapMessage(m => m.Type is MessageType.Text && predicate(m.Text!), handler);
        }

        /// <summary>
        /// Maps an incoming text message command evaluating against a custom functional predicate 
        /// to a flexible generic execution delegate.
        /// </summary>
        /// <param name="predicate">The evaluation logic applied to the extracted text message content to verify if the command matches.</param>
        /// <param name="handler">An open-signature delegate framework (e.g., minimalist lambda parameters) targeted for update execution mapping.</param>
        /// <returns>A <see cref="MessageHandlerBuilder"/> instance used to further configure or chain configurations for the mapped message pipeline.</returns>
        public MessageHandlerBuilder MapCommand(Func<string, bool> predicate, Delegate handler)
        {
            return app.MapMessage(m => m.Type is MessageType.Text && predicate(m.Text!), handler);
        }

        /// <summary>
        /// Maps an absolute string pattern to a target handler delegate by executing a non-case-sensitive comparison against incoming text contents.
        /// </summary>
        /// <param name="command">The exact targeted command string literal (e.g., "/start") to match against.</param>
        /// <param name="handler">An open-signature delegate framework triggered whenever the command pattern is intercepted.</param>
        /// <returns>A <see cref="MessageHandlerBuilder"/> instance used to further configure or chain configurations for the mapped message pipeline.</returns>
        /// <remarks>
        /// Matching is structurally executed via <see cref="StringComparison.InvariantCultureIgnoreCase"/> to robustly guarantee 
        /// seamless parsing across heterogeneous system culture variants.
        /// </remarks>
        public MessageHandlerBuilder MapCommand(string command, Delegate handler)
        {
            return app.MapCommand(c => c.Equals(command, StringComparison.InvariantCultureIgnoreCase), handler);
        }
    }
}