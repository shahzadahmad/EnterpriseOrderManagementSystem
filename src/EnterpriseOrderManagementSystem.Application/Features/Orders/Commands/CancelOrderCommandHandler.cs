using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;

public sealed class CancelOrderCommandHandler
    : IRequestHandler<CancelOrderCommand, Guid>
{
    #region Fields

    private readonly IOrderRepository _orderRepository;

    #endregion

    #region Constructor

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(
            request.OrderId,
            cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(
                "Order",
                request.OrderId);
        }

        order.Cancel(request.Reason);

        return order.Id;
    }

    #endregion
}