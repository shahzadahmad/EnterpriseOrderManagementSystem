using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Common.Events;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseOrderManagementSystem.Infrastructure.Services.DomainEvents;

public sealed class DomainEventDispatcher
    : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    #region Dispatch

    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await DispatchEventAsync(
                domainEvent,
                cancellationToken);
        }
    }

    #endregion

    #region Dispatch Event

    private async Task DispatchEventAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var handlerType =
            typeof(IDomainEventHandler<>)
                .MakeGenericType(domainEvent.GetType());

        var handlers =
            _serviceProvider
                .GetServices(handlerType);

        foreach (var handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            var handleMethod =
                handlerType.GetMethod(
                    nameof(IDomainEventHandler<IDomainEvent>.HandleAsync));

            if (handleMethod is null)
            {
                throw new InvalidOperationException(
                    $"HandleAsync method was not found on " +
                    $"{handlerType.Name}.");
            }

            var task =
                (Task?)handleMethod.Invoke(
                    handler,
                    new object[]
                    {
                        domainEvent,
                        cancellationToken
                    });

            if (task is null)
            {
                throw new InvalidOperationException(
                    $"Domain event handler " +
                    $"{handler.GetType().Name} returned null.");
            }

            await task;
        }
    }

    #endregion
}