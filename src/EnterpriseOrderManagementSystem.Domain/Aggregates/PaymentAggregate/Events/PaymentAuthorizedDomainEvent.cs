using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate.Events;

/// <summary>
/// Raised when a payment is successfully authorized
/// by the payment provider.
/// </summary>
public sealed record PaymentAuthorizedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string ProviderReference) : DomainEvent;