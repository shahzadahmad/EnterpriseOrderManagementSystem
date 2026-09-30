using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Repositories;

namespace EnterpriseOrderManagementSystem.Application.EventHandlers.Payments;

/// <summary>Handles application-level notification for PaymentCreatedDomainEvent.</summary>
public sealed class PaymentCreatedDomainEventHandler : IDomainEventHandler<PaymentCreatedDomainEvent>
{
    private readonly IOrderRepository _orders;
    private readonly ICustomerRepository _customers;
    private readonly IEmailService _emailService;

    public PaymentCreatedDomainEventHandler(IOrderRepository orders, ICustomerRepository customers, IEmailService emailService)
    {
        _orders = orders;
        _customers = customers;
        _emailService = emailService;
    }

    public async Task HandleAsync(PaymentCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(domainEvent.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), domainEvent.OrderId);
        var customer = await _customers.GetByIdAsync(order.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), order.CustomerId);

        await _emailService.SendAsync(
            customer.Email.ToString(),
            "Payment created",
            $"Your payment for order {domainEvent.OrderId} has been created. Amount: {domainEvent.Amount} {domainEvent.Currency}.",
            cancellationToken);
    }
}
