namespace TeleFrame.UpdateHandlers.MessageHandlers.VoiceHandlers;

public static class VoiceHandlerExtensions
{
    extension(TelegramBotApplication app)
    {
        public MessageHandlerBuilder MapVoice(Delegate handler)
        {
            return app.MapVoice(_ => true, handler);
        }

        public MessageHandlerBuilder MapVoice(Func<Voice, bool> predicate, UpdateHandlerDelegate handler)
        {
            return app.MapMessage(m => m.Type is MessageType.Voice && predicate(m.Voice!), handler);
        }

        public MessageHandlerBuilder MapVoice(Func<Voice, bool> predicate, Delegate handler)
        {
            return app.MapMessage(m => m.Type is MessageType.Voice && predicate(m.Voice!), handler);
        }
    }
}