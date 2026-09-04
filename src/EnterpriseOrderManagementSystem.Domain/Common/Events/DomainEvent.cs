namespace EnterpriseOrderManagementSystem.Domain.Common.Events;

/// <summary>
/// Base implementation for all domain events.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}