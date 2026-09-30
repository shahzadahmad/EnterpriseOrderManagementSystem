using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Customers;
using EnterpriseOrderManagementSystem.Application.Mapping;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Customers.Queries;

public sealed class GetCustomersQueryHandler
    : IRequestHandler<
        GetCustomersQuery,
        PaginatedResult<CustomerDto>>
{
    #region Fields

    private readonly IReadRepository<Customer> _readRepository;

    #endregion

    #region Constructor

    public GetCustomersQueryHandler(
        IReadRepository<Customer> readRepository)
    {
        _readRepository = readRepository;
    }

    #endregion

    #region Handle

    public async Task<PaginatedResult<CustomerDto>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;

        var totalCount = await _readRepository.CountAsync(
            cancellationToken);

        var customers = await _readRepository.ListAsync(
            pagination.Skip,
            pagination.PageSize,
            cancellationToken);

        var items = customers
            .Select(customer => customer.ToDto())
            .ToArray();

        return new PaginatedResult<CustomerDto>(
            items,
            pagination.PageNumber,
            pagination.PageSize,
            totalCount);
    }

    #endregion

}