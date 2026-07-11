namespace TeleFrame.Results.Abstractions;

public interface ITelegramResult
{
    public Task InvokeAsync(UpdateContext ctx);
}