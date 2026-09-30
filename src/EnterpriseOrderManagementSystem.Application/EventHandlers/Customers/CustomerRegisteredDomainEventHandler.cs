using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Repositories;

namespace EnterpriseOrderManagementSystem.Application.EventHandlers.Customers;

/// <summary>Handles customer registration notifications.</summary>
public sealed class CustomerRegisteredDomainEventHandler : IDomainEventHandler<CustomerRegisteredDomainEvent>
{
    private readonly ICustomerRepository _customers;
    private readonly IEmailService _emailService;
    public CustomerRegisteredDomainEventHandler(ICustomerRepository customers, IEmailService emailService) { _customers = customers; _emailService = emailService; }
    public async Task HandleAsync(CustomerRegisteredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var customer = await _customers.GetByIdAsync(domainEvent.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), domainEvent.CustomerId);

        await _emailService.SendAsync(customer.Email.ToString(), "Welcome to Enterprise Order Management", $"Hello {customer.FirstName} {customer.LastName}, your customer account has been registered successfully.", cancellationToken);
    }
}
