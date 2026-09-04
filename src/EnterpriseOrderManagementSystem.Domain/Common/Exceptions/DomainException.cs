namespace EnterpriseOrderManagementSystem.Domain.Common.Exceptions;

/// <summary>
/// Represents the base exception for all domain-related errors.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }

    protected DomainException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}