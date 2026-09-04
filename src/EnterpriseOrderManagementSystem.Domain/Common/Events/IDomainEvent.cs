namespace EnterpriseOrderManagementSystem.Domain.Common.Events;

/// <summary>
/// Represents a domain event raised by the domain model.
///
/// Domain events describe business facts that have already occurred.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier of the event.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// UTC timestamp when the event occurred.
    /// </summary>
    DateTime OccurredOnUtc { get; }
}