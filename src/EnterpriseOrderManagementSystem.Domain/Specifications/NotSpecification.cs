namespace EnterpriseOrderManagementSystem.Domain.Specifications;

/// <summary>
/// Represents the logical negation of another specification.
/// </summary>
/// <typeparam name="T">
/// The domain type evaluated by the specification.
/// </typeparam>
public sealed class NotSpecification<T> : Specification<T>
{
    #region Fields

    private readonly Specification<T> _specification;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new NOT specification.
    /// </summary>
    public NotSpecification(
        Specification<T> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);

        _specification = specification;
    }

    #endregion

    #region Evaluation

    /// <summary>
    /// Returns true when the wrapped specification
    /// is not satisfied.
    /// </summary>
    public override bool IsSatisfiedBy(T entity)
    {
        return !_specification.IsSatisfiedBy(entity);
    }

    #endregion
}