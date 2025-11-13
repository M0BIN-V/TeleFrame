using Telegram.Bot.Types.Enums;

namespace TeleFrame.UpdateHandlers.MessageHandlers.CommandHandlers;

public static class CommandHandlerExtensions
{
    extension(TelegramBotApplication app)
    {
        public MessageHandlerBuilder MapCommand(Func<string, bool> predicate, UpdateHandlerDelegate handler)
        {
            return app.MapMessage(m => m.Type is MessageType.Text && predicate(m.Text!), handler);
        }

        public MessageHandlerBuilder MapCommand(Func<string, bool> predicate, Delegate handler)
        {
            return app.MapMessage(m => m.Type is MessageType.Text && predicate(m.Text!), handler);
        }

        public MessageHandlerBuilder MapCommand(string command, Delegate handler)
        {
            return app.MapCommand(c => c.Equals(command, StringComparison.InvariantCultureIgnoreCase), handler);
        }
    }
}