namespace TeleFrame.UpdateHandlers.MessageHandlers;

/// <summary>
///     Represents a specialized builder for configuring and fluidly chaining rules for message-specific update handlers.
/// </summary>
/// <remarks>
///     This class inherits from the base <see cref="UpdateHandlerBuilder" /> and extends it with state-aware mechanics
///     to facilitate Finite State Machine (FSM) routing within conversation flows.
/// </remarks>
/// <param name="handler">
///     The underlying execution delegate processed when all message criteria and state constraints are
///     met.
/// </param>
public class MessageHandlerBuilder(UpdateHandlerDelegate handler) : UpdateHandlerBuilder(handler)
{
    /// <summary>
    ///     Gets the required conversation state string that a user must currently possess in order to trigger this handler.
    /// </summary>
    /// <value>
    ///     The key identifying the target conversational state. Defaults to an empty string indicating no state
    ///     restriction.
    /// </value>
    public string RequireStateText { get; private set; } = "";

    /// <summary>
    ///     Restricts the execution of this message handler to users who are currently bound to a specific conversational
    ///     state.
    /// </summary>
    /// <param name="state">
    ///     The tracking state key or identifier (e.g., "AwaitingPassword", "Step_2") required to execute this
    ///     block.
    /// </param>
    /// <returns>The current <see cref="MessageHandlerBuilder" /> instance to support fluent method chaining configurations.</returns>
    /// <remarks>
    ///     This property is typically validated upstream against the state manager context prior to firing the inner pipeline
    ///     execution.
    /// </remarks>
    public MessageHandlerBuilder RequireState(string state)
    {
        RequireStateText = state;
        return this;
    }
}