namespace TeleFrame.UpdateHandlers;

public interface IUpdateHandlerFilter
{
    Task InvokeAsync(UpdateContext context, UpdateHandlerFilterDelegate next, CancellationToken ct);
}