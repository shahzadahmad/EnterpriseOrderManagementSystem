using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Application.DependencyInjection;
using EnterpriseOrderManagementSystem.Domain.Common.Events;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseOrderManagementSystem.Application.Tests.EventHandlers;

public sealed class DomainEventDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_WhenHandlerIsRegistered_DispatchesEventToHandler()
    {
        var services = new ServiceCollection();
        var handler = new TestDomainEventHandler();
        services.AddSingleton<IDomainEventHandler<TestDomainEvent>>(handler);
        var provider = services.BuildServiceProvider();
        var dispatcher = new DomainEventDispatcher(provider);
        var domainEvent = new TestDomainEvent(Guid.NewGuid());

        await dispatcher.DispatchAsync([domainEvent]);

        Assert.Equal(domainEvent.EventId, handler.HandledEventId);
    }

    [Fact]
    public async Task DispatchAsync_WhenNoHandlerIsRegistered_CompletesWithoutError()
    {
        var provider = new ServiceCollection().BuildServiceProvider();
        var dispatcher = new DomainEventDispatcher(provider);

        await dispatcher.DispatchAsync([new TestDomainEvent(Guid.NewGuid())]);
    }

    private sealed record TestDomainEvent(Guid AggregateId) : DomainEvent;

    private sealed class TestDomainEventHandler : IDomainEventHandler<TestDomainEvent>
    {
        public Guid? HandledEventId { get; private set; }

        public Task HandleAsync(TestDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            HandledEventId = domainEvent.EventId;
            return Task.CompletedTask;
        }
    }
}
