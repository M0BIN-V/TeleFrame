namespace TeleFrame.Services.StateManager;

public static class StateManagerExtensions
{
    public static void ClearState(this IStateManager manager)
    {
        manager.SetState(string.Empty);
    }

    public static bool StateIsEmpty(this IStateManager manager)
    {
        return manager.State == string.Empty;
    }

    public static bool StateIs(this IStateManager manager, string state)
    {
        return manager.State == state;
    }
}