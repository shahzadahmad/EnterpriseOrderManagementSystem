using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Common.Events;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseOrderManagementSystem.Application.DependencyInjection;

/// <summary>
/// Dispatches domain events to all registered Application-layer handlers.
///
/// The dispatcher deliberately depends only on the handler abstraction and
/// IServiceProvider. Concrete infrastructure concerns remain outside the
/// Application layer.
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;
                var task = (Task)method.Invoke(handler, [domainEvent, cancellationToken])!;
                await task.ConfigureAwait(false);
            }
        }
    }
}
