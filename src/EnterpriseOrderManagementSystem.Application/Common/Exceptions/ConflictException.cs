namespace EnterpriseOrderManagementSystem.Application.Common.Exceptions;

/// <summary>
/// Represents a business conflict with the current state of a resource.
/// </summary>
public sealed class ConflictException : ApplicationException
{
    public ConflictException(
        string message)
        : base(message)
    {
    }
}