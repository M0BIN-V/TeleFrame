namespace TeleFrame.Services.StateManager;

public interface IStateManager
{
    public string State { get; }
    public void SetState(string state);
}