using TeleFrame.Application;
using TeleFrame.Services;
using TeleFrame.UpdateHandlers.CommandHandlers;

namespace TeleFrame.UpdateHandlers;

public static class UpdateHandlerExtensions
{
    extension(TelegramBotApplication app)
    {
        public UpdateHandlerBuilder MapUpdate(Func<UpdateContext, bool> predicate, Delegate handler)
        {
            var builder = new UpdateHandlerBuilder(UpdateHandlerFactory.Create(handler));
        
            app.MapUpdate(predicate, ((context, ct) => builder.Build().Invoke(context, ct)));

            return builder;
        }

        public UpdateHandlerBuilder MapUpdate(Func<UpdateContext, bool> predicate, UpdateHandlerDelegate handler)
        {
            var builder = new UpdateHandlerBuilder(handler);

            app.Use(next => async (context, ct) =>
            {
                if (predicate(context))
                {
                    var finalHandler = builder.Build();
                    await finalHandler(context, ct);
                }
                else
                {
                    await next(context, ct);
                }
            });

            return builder;
        }
    }
}