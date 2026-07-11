namespace TeleFrame.Services.StateManager;

/// <summary>
/// Defines the architectural contract for managing and tracking the conversation state of a Telegram user.
/// </summary>
/// <remarks>
/// Implementations of this interface (such as memory-based or distributed cache providers) 
/// act as the core engine for stateful conversation tracking, enabling Finite State Machine (FSM) routing mechanisms.
/// </remarks>
public interface IStateManager
{
    /// <summary>
    /// Gets the current conversational state key identifier associated with the user in the current scope.
    /// </summary>
    /// <value>A string representing the current state (e.g., "AwaitingEmail", "MainPage"), or an empty string if no active state exists.</value>
    public string State { get; }

    /// <summary>
    /// Transitions the user's conversational context into a new designated state.
    /// </summary>
    /// <param name="state">The target state string token or key to persist for the user.</param>
    /// <remarks>
    /// Setting this to an empty string or a specific clear-token usually signifies resetting the user back to the default/idle state.
    /// </remarks>
    public void SetState(string state);
}