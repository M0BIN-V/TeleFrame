namespace TeleFrame.UpdateHandlers.MessageHandlers;

public class MessageHandlerBuilder : UpdateHandlerBuilder
{
    public MessageHandlerBuilder(UpdateHandlerDelegate handler) : base(handler) { }

    public string RequireStateText { get; private set; } = "";


    public MessageHandlerBuilder RequireState(string state)
    {
        RequireStateText = state;
        return this;
    }
}