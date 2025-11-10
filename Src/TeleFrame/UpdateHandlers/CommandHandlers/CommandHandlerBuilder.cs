namespace TeleFrame.UpdateHandlers.CommandHandlers;

public class CommandHandlerBuilder : UpdateHandlerBuilder
{
    public CommandHandlerBuilder(UpdateHandlerDelegate handler) : base(handler) { }

    public string RequireStateText { get; private set; } = "";


    public UpdateHandlerBuilder RequireState(string state)
    {
        RequireStateText = state;
        return this;
    }
}