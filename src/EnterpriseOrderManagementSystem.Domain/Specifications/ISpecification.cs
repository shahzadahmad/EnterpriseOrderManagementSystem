namespace EnterpriseOrderManagementSystem.Domain.Specifications;

/// <summary>
/// Defines a business specification that determines
/// whether an object satisfies a particular rule.
/// </summary>
/// <typeparam name="T">
/// The type to which the specification applies.
/// </typeparam>
public interface ISpecification<T>
{
    #region Evaluation

    /// <summary>
    /// Determines whether the supplied object satisfies
    /// this specification.
    /// </summary>
    bool IsSatisfiedBy(T entity);

    #endregion
}