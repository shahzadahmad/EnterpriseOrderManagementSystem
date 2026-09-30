using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;

public sealed class ConfirmOrderCommandHandler
    : IRequestHandler<ConfirmOrderCommand, Guid>
{
    #region Fields

    private readonly IOrderRepository _orderRepository;

    #endregion

    #region Constructor

    public ConfirmOrderCommandHandler(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        ConfirmOrderCommand request,
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

        order.Confirm();

        return order.Id;
    }

    #endregion
}
