namespace EnterpriseOrderManagementSystem.Domain.Common.Base;

/// <summary>
/// Represents the base class for all domain entities.
///
/// An entity is uniquely identified by its identifier rather than
/// by the values of its properties.
///
/// Two entities are considered equal when they have the same
/// identifier and belong to the same runtime type.
/// </summary>
/// <typeparam name="TKey">
/// Type of the entity identifier (Guid, long, int, etc.).
/// </typeparam>
public abstract class Entity<TKey> : IEquatable<Entity<TKey>>
    where TKey : notnull
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    public TKey Id { get; protected set; } = default!;

    /// <summary>
    /// Determines whether this entity has not yet been assigned
    /// a persistent identifier.
    /// </summary>
    public bool IsTransient()
    {
        return EqualityComparer<TKey>.Default.Equals(Id, default!);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TKey> other)
            return false;

        return Equals(other);
    }

    /// <inheritdoc/>
    public bool Equals(Entity<TKey>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (IsTransient() || other.IsTransient())
            return false;

        return EqualityComparer<TKey>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (IsTransient())
            return base.GetHashCode();

        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(
        Entity<TKey>? left,
        Entity<TKey>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(
        Entity<TKey>? left,
        Entity<TKey>? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Returns a readable representation of the entity.
    /// </summary>
    public override string ToString()
    {
        return $"{GetType().Name} [Id={Id}]";
    }
}