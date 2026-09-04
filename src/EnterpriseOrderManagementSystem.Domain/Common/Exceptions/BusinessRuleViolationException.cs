namespace EnterpriseOrderManagementSystem.Domain.Common.Exceptions;

/// <summary>
/// Represents a generic business rule violation.
/// </summary>
public sealed class BusinessRuleViolationException
    : DomainException
{
    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }
}