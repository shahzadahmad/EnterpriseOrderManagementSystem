namespace EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;

public interface IReadRepository<T>
    where T : class
{
    Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<T>> ListAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        CancellationToken cancellationToken = default);
}