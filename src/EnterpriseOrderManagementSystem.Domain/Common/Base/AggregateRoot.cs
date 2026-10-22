using EnterpriseOrderManagementSystem.Domain.Common.Events;
// Add the using for IHasDomainEvents if it is in another namespace.

namespace EnterpriseOrderManagementSystem.Domain.Common.Base;

/// <summary>
/// Represents the base class for all aggregate roots.
///
/// Aggregate roots are responsible for maintaining business
/// invariants and raising domain events.
/// </summary>
/// <typeparam name="TKey">
/// Type of the aggregate identifier.
/// </typeparam>
public abstract class AggregateRoot<TKey>
    : Entity<TKey>, IHasDomainEvents
    where TKey : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the domain events raised by this aggregate.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        if (_domainEvents.Contains(domainEvent))
            return;

        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a domain event.
    /// </summary>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Removes all pending domain events.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Determines whether the aggregate has pending domain events.
    /// </summary>
    public bool HasDomainEvents()
    {
        return _domainEvents.Count > 0;
    }
}