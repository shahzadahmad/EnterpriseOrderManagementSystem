namespace EnterpriseOrderManagementSystem.Domain.Specifications;

/// <summary>
/// Represents a specification composed of two specifications
/// using logical AND.
/// </summary>
/// <typeparam name="T">
/// The domain type evaluated by the specifications.
/// </typeparam>
public sealed class AndSpecification<T> : Specification<T>
{
    #region Fields

    private readonly Specification<T> _left;

    private readonly Specification<T> _right;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new AND specification.
    /// </summary>
    public AndSpecification(
        Specification<T> left,
        Specification<T> right)
    {
        ArgumentNullException.ThrowIfNull(left);

        ArgumentNullException.ThrowIfNull(right);

        _left = left;

        _right = right;
    }

    #endregion

    #region Evaluation

    /// <summary>
    /// Returns true only when both specifications
    /// are satisfied.
    /// </summary>
    public override bool IsSatisfiedBy(T entity)
    {
        return _left.IsSatisfiedBy(entity)
            && _right.IsSatisfiedBy(entity);
    }

    #endregion
}