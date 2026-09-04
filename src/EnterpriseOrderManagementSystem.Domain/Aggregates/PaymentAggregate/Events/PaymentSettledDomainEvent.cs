using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate.Events;

/// <summary>
/// Raised when a captured payment is successfully settled.
/// </summary>
public sealed record PaymentSettledDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string ProviderReference) : DomainEvent;