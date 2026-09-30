namespace EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;

public interface IQuerySpecification<T>
{
    int? Skip { get; }

    int? Take { get; }

    bool AsNoTracking { get; }
}