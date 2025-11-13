using TeleFrame.Services.StateManager;
using Telegram.Bot.Types.Enums;

namespace TeleFrame.UpdateHandlers.MessageHandlers;

public static class MessageHandlerExtensions
{
    extension(TelegramBotApplication app)
    {
        public MessageHandlerBuilder MapMessage(Func<Message, bool> predicate, UpdateHandlerDelegate handler)
        {
            var builder = new MessageHandlerBuilder(handler);

            app.MapUpdate(u =>
                    u.Update.Type == UpdateType.Message &&
                    u.Update.Message!.Text != null &&
                    u.Services.GetRequiredService<IStateManager>().StateIs(builder.RequireStateText) &&
                    predicate(u.Update.Message),
                (context, ct) => builder.Build().Invoke(context, ct));

            return builder;
        }

        public MessageHandlerBuilder MapMessage(Func<Message, bool> predicate, Delegate handler)
        {
            var convertedHandler = UpdateHandlerFactory.Create(handler);
            return app.MapMessage(predicate, convertedHandler);
        }

        public MessageHandlerBuilder MapMessage(MessageType type, Delegate handler)
        {
            return app.MapMessage(m => m.Type == type, handler);
        }
    }
}