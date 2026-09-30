using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Orders;
using EnterpriseOrderManagementSystem.Application.Mapping;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Queries;

public sealed class GetOrdersQueryHandler
    : IRequestHandler<
        GetOrdersQuery,
        PaginatedResult<OrderDto>>
{
    #region Fields

    private readonly IReadRepository<Order> _readRepository;

    #endregion

    #region Constructor

    public GetOrdersQueryHandler(
        IReadRepository<Order> readRepository)
    {
        _readRepository = readRepository;
    }

    #endregion

    #region Handle

    public async Task<PaginatedResult<OrderDto>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;

        var totalCount = await _readRepository.CountAsync(
            cancellationToken);

        var orders = await _readRepository.ListAsync(
            pagination.Skip,
            pagination.PageSize,
            cancellationToken);

        var items = orders
            .Select(order => order.ToDto())
            .ToArray();

        return new PaginatedResult<OrderDto>(
            items,
            pagination.PageNumber,
            pagination.PageSize,
            totalCount);
    }

    #endregion
}
