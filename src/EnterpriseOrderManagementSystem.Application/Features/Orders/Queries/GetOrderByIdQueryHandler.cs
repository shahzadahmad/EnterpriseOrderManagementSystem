using EnterpriseOrderManagementSystem.Application.DTOs.Orders;
using EnterpriseOrderManagementSystem.Application.Mapping;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Queries;

public sealed class GetOrderByIdQueryHandler
    : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    #region Fields

    private readonly IOrderRepository _orderRepository;

    #endregion

    #region Constructor

    public GetOrderByIdQueryHandler(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    #endregion

    #region Handle

    public async Task<OrderDto?> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(
            request.OrderId,
            cancellationToken);

        return order?.ToDto();
    }

    #endregion
}