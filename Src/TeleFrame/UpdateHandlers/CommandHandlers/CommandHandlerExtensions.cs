using Microsoft.Extensions.DependencyInjection;
using TeleFrame.Application;
using TeleFrame.Services.StateManager;
using Telegram.Bot.Types.Enums;

namespace TeleFrame.UpdateHandlers.CommandHandlers;

public static class CommandHandlerExtensions
{
    public static CommandHandlerBuilder MapCommand(
        this TelegramBotApplication app,
        string command,
        Delegate handler)
    {
        return app.MapCommand(c => c.Equals(command, StringComparison.OrdinalIgnoreCase), handler);
    }

    public static CommandHandlerBuilder MapCommand(
        this TelegramBotApplication app,
        Func<string, bool> predicate,
        Delegate handler)
    {
        var commandHandler = UpdateHandlerFactory.Create(handler);
        return app.MapCommand(predicate, commandHandler);
    }

    public static CommandHandlerBuilder MapCommand(
        this TelegramBotApplication app,
        Func<string, bool> predicate,
        UpdateHandlerDelegate handler)
    {
        var builder = new CommandHandlerBuilder(handler);
        
        app.MapUpdate(u =>
            u.Update.Type == UpdateType.Message &&
            u.Update.Message!.Text != null &&
            u.Services.GetRequiredService<IStateManager>().StateIs(builder.RequireStateText) &&
            predicate(u.Update.Message.Text),
            ((context, ct) => builder.Build().Invoke(context, ct)));

        return builder;
    }
}