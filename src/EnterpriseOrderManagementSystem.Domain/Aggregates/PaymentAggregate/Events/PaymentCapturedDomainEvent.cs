using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate.Events;

/// <summary>
/// Raised when an authorized payment is successfully captured.
/// </summary>
public sealed record PaymentCapturedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string ProviderReference) : DomainEvent;