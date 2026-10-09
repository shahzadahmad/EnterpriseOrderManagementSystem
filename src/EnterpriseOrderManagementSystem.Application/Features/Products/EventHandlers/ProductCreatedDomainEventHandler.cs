using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate.Events;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.EventHandlers;

public sealed class ProductCreatedDomainEventHandler
    : IDomainEventHandler<ProductCreatedDomainEvent>
{
    public async Task HandleAsync(
        ProductCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        // Future application behavior.

        await Task.CompletedTask;
    }
}