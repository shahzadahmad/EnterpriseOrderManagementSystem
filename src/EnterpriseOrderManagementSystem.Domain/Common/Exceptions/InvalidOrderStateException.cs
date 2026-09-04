namespace EnterpriseOrderManagementSystem.Domain.Common.Exceptions;

/// <summary>
/// Thrown when an invalid state transition is attempted.
/// </summary>
public sealed class InvalidOrderStateException
    : DomainException
{
    public InvalidOrderStateException(
        string currentState,
        string attemptedAction)
        : base(
            $"Cannot perform '{attemptedAction}' " +
            $"while order is '{currentState}'.")
    {
    }
}