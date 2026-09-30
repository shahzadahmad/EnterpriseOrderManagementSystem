using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Repositories;

namespace EnterpriseOrderManagementSystem.Application.EventHandlers.Orders;

/// <summary>Handles application-level notification for OrderConfirmedDomainEvent.</summary>
public sealed class OrderConfirmedDomainEventHandler : IDomainEventHandler<OrderConfirmedDomainEvent>
{
    private readonly IOrderRepository _orders;
    private readonly ICustomerRepository _customers;
    private readonly IEmailService _emailService;

    public OrderConfirmedDomainEventHandler(IOrderRepository orders, ICustomerRepository customers, IEmailService emailService)
    {
        _orders = orders;
        _customers = customers;
        _emailService = emailService;
    }

    public async Task HandleAsync(OrderConfirmedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(domainEvent.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), domainEvent.OrderId);
        var customer = await _customers.GetByIdAsync(order.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), order.CustomerId);

        await _emailService.SendAsync(
            customer.Email.ToString(),
            "Order confirmed",
            $"Your order {domainEvent.OrderId} has been confirmed.",
            cancellationToken);
    }
}
