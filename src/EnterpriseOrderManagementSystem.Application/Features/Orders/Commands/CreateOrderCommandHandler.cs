using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;

public sealed class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, Guid>
{
    #region Fields

    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;

    #endregion

    #region Constructor

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var customerExists = await _customerRepository.ExistsAsync(
            request.CustomerId,
            cancellationToken);

        if (!customerExists)
        {
            throw new NotFoundException(
                "Customer",
                request.CustomerId);
        }

        var order = Order.Create(request.CustomerId);

        await _orderRepository.AddAsync(
            order,
            cancellationToken);

        return order.Id;
    }

    #endregion
}