using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Common.Events;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that wraps command execution
/// inside a database transaction.
///
/// Transaction flow:
/// 1. Begin transaction
/// 2. Execute the command handler
/// 3. Persist changes
/// 4. Capture pending domain events
/// 5. Commit transaction
/// 6. Clear dispatched events and dispatch after commit
/// 7. Roll back if persistence fails
///
/// This behavior applies only to commands because commands
/// are responsible for changing application state.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher domainEventDispatcher)
    {
        _unitOfWork = unitOfWork;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Start a transaction before executing the command handler.
        // All database changes made by the handler will be part
        // of the same transaction.
        await _unitOfWork.BeginTransactionAsync(
            cancellationToken);

        TResponse response;
        IReadOnlyCollection<IDomainEvent> domainEvents;

        try
        {
            // Execute the command and persist its aggregate changes.
            response = await next(cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Snapshot events while the aggregates are still tracked.
            domainEvents = _unitOfWork.GetDomainEvents();

            // Events are dispatched only after the database transaction commits.
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Clear only after a successful commit so failed persistence does not
            // silently discard events from tracked aggregates.
            _unitOfWork.ClearDomainEvents();
        }
        catch
        {
            // Persistence failures must roll back. A post-commit dispatch failure
            // occurs outside this block and cannot undo an already committed DB transaction.
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        if (domainEvents.Count > 0)
        {
            await _domainEventDispatcher.DispatchAsync(
                domainEvents,
                cancellationToken);
        }

        return response;
    }
}