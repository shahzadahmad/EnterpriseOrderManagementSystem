using EnterpriseOrderManagementSystem.Application.Behaviors;
using FluentValidation;
using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Common.Events;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseOrderManagementSystem.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);

            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            configuration.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        #region Domain Event Handler Registration

        // Register every custom IDomainEventHandler<TDomainEvent> implementation
        // found in the Application assembly so the dispatcher can resolve it.
        var domainEventHandlerTypes = assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .SelectMany(type => type.GetInterfaces()
                .Where(@interface => @interface.IsGenericType
                    && @interface.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                .Select(@interface => new { Service = @interface, Implementation = type }));

        foreach (var handlerType in domainEventHandlerTypes)
        {
            services.AddScoped(handlerType.Service, handlerType.Implementation);
        }

        #endregion

        return services;
    }
}