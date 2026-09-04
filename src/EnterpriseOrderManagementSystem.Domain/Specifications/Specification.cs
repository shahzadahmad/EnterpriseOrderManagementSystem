namespace EnterpriseOrderManagementSystem.Domain.Specifications;

/// <summary>
/// Base class for domain specifications.
///
/// Provides reusable logical composition of specifications
/// using AND, OR and NOT operations.
/// </summary>
/// <typeparam name="T">
/// The domain type evaluated by the specification.
/// </typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    #region Evaluation

    /// <summary>
    /// Determines whether the supplied entity satisfies
    /// this specification.
    /// </summary>
    public abstract bool IsSatisfiedBy(T entity);

    #endregion

    #region Composition

    /// <summary>
    /// Combines this specification with another specification
    /// using logical AND.
    /// </summary>
    public Specification<T> And(
        Specification<T> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return new AndSpecification<T>(
            this,
            specification);
    }

    /// <summary>
    /// Combines this specification with another specification
    /// using logical OR.
    /// </summary>
    public Specification<T> Or(
        Specification<T> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return new OrSpecification<T>(
            this,
            specification);
    }

    /// <summary>
    /// Negates this specification using logical NOT.
    /// </summary>
    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }

    #endregion
}