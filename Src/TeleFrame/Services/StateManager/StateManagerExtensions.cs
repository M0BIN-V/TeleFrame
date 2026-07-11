namespace TeleFrame.Services.StateManager;

/// <summary>
/// Provides fluent extension members utilizing modern C# extension syntax to simplify 
/// state manipulation and evaluation on <see cref="IStateManager"/> instances.
/// </summary>
public static class StateManagerExtensions
{
    extension(IStateManager manager)
    {
        /// <summary>
        /// Resets the current conversational state back to the default idle or unassigned state.
        /// </summary>
        /// <remarks>
        /// Internally updates the state storage to an empty string, effectively clearing any FSM restriction for the user.
        /// </remarks>
        public void ClearState()
        {
            manager.SetState(string.Empty);
        }

        /// <summary>
        /// Validates whether the user currently occupies no specific conversational state.
        /// </summary>
        /// <returns><see langword="true"/> if the current state is empty; otherwise, <see langword="false"/>.</returns>
        public bool StateIsEmpty()
        {
            return manager.State == string.Empty;
        }

        /// <summary>
        /// Evaluates whether the current conversational state matches a specific targeted state token.
        /// </summary>
        /// <param name="state">The target state identifier token to compare against.</param>
        /// <returns><see langword="true"/> if the manager's current state matches the provided <paramref name="state"/> parameter; otherwise, <see langword="false"/>.</returns>
        public bool StateIs(string state)
        {
            return manager.State == state;
        }
    }
}