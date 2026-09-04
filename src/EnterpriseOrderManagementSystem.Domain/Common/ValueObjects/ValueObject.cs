namespace EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;

/// <summary>
/// Represents the base class for all value objects.
///
/// Value objects are immutable and are compared by the values
/// of their properties rather than by identity.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Returns the values that participate in equality comparison.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj)
        => Equals(obj as ValueObject);

    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(
        ValueObject? left,
        ValueObject? right)
        => Equals(left, right);

    public static bool operator !=(
        ValueObject? left,
        ValueObject? right)
        => !Equals(left, right);
}