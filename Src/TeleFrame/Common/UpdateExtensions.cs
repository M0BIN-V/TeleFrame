using TeleFrame.Exceptions;

namespace TeleFrame.Common;

public static class UpdateExtensions
{
    public static ChatId ResolveChatId(this Update update)
    {
        return update.Type switch
        {
            UpdateType.Message => update.Message!.Chat.Id,
            UpdateType.EditedMessage => update.EditedMessage!.Chat.Id,
            UpdateType.ChannelPost => update.ChannelPost!.Chat.Id,
            _ => throw new FailedToResolveChatId()
        };
    }
}