using TeleFrame.Services;

namespace TeleFrame.Results;

public interface ITelegramResult
{
    public Task InvokeAsync(UpdateContext ctx);
}