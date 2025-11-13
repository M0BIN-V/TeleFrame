namespace TeleFrame.Services.StateManager;

public static class StateManagerExtensions
{
    extension(IStateManager manager)
    {
        public void ClearState()
        {
            manager.SetState(string.Empty);
        }

        public bool StateIsEmpty()
        {
            return manager.State == string.Empty;
        }

        public bool StateIs(string state)
        {
            return manager.State == state;
        }
    }
}