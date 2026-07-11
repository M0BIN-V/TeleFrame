namespace TeleFrame.UpdateHandlers.MessageHandlers.VoiceHandlers;

/// <summary>
///     Provides extension members utilizing modern C# extension syntax to fluently register
///     voice-message handlers directly onto the <see cref="TelegramBotApplication" /> pipeline.
/// </summary>
public static class VoiceHandlerExtensions
{
    extension(TelegramBotApplication app)
    {
        /// <summary>
        ///     Maps any incoming voice message payload unconditionally to a generic execution delegate.
        /// </summary>
        /// <param name="handler">An open-signature delegate framework triggered whenever a voice message type is received.</param>
        /// <returns>
        ///     A <see cref="MessageHandlerBuilder" /> instance used to further configure or chain configurations for the
        ///     mapped message pipeline.
        /// </returns>
        public MessageHandlerBuilder MapVoice(Delegate handler)
        {
            return app.MapVoice(_ => true, handler);
        }

        /// <summary>
        ///     Maps an incoming voice message metadata payload evaluating against a custom functional predicate
        ///     to a strongly-typed update handler delegate.
        /// </summary>
        /// <param name="predicate">
        ///     The evaluation criteria applied to the extracted <see cref="Voice" /> instance (e.g., checking
        ///     duration or file size).
        /// </param>
        /// <param name="handler">The core delegate execution logic triggered when the voice message matches the criteria.</param>
        /// <returns>
        ///     A <see cref="MessageHandlerBuilder" /> instance used to further configure or chain configurations for the
        ///     mapped message pipeline.
        /// </returns>
        /// <remarks>
        ///     This method automatically enforces pre-filtering to ensure that incoming updates are explicitly structured as
        ///     <see cref="MessageType.Voice" /> variants prior to invoking the predicate safely via null-suppression (
        ///     <c>m.Voice!</c>).
        /// </remarks>
        public MessageHandlerBuilder MapVoice(Func<Voice, bool> predicate, UpdateHandlerDelegate handler)
        {
            return app.MapMessage(m => m.Type is MessageType.Voice && predicate(m.Voice!), handler);
        }

        /// <summary>
        ///     Maps an incoming voice message metadata payload evaluating against a custom functional predicate
        ///     to a flexible generic execution delegate.
        /// </summary>
        /// <param name="predicate">
        ///     The evaluation criteria applied to the extracted <see cref="Voice" /> instance (e.g., checking
        ///     duration or file size).
        /// </param>
        /// <param name="handler">
        ///     An open-signature delegate framework (e.g., minimalist lambda parameters) targeted for update
        ///     execution mapping.
        /// </param>
        /// <returns>
        ///     A <see cref="MessageHandlerBuilder" /> instance used to further configure or chain configurations for the
        ///     mapped message pipeline.
        /// </returns>
        public MessageHandlerBuilder MapVoice(Func<Voice, bool> predicate, Delegate handler)
        {
            return app.MapMessage(m => m.Type is MessageType.Voice && predicate(m.Voice!), handler);
        }
    }
}