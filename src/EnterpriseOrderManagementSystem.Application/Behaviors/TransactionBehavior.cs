using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Repositories;
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
/// 4. Commit transaction
/// 5. Roll back if any operation fails
///
/// This behavior applies only to commands because commands
/// are responsible for changing application state.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

        try
        {
            // Execute the actual command handler.
            var response = await next(cancellationToken);

            // Persist all tracked entity changes to the database.
            // This is intentionally done before committing the transaction.
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            // Commit only after the handler has completed successfully
            // and all changes have been saved.
            await _unitOfWork.CommitTransactionAsync(
                cancellationToken);

            return response;
        }
        catch
        {
            // If the handler, SaveChanges, or Commit operation fails,
            // roll back the transaction so partial changes are not persisted.
            await _unitOfWork.RollbackTransactionAsync(
                cancellationToken);

            // Preserve the original exception and allow the
            // application's exception-handling pipeline to process it.
            throw;
        }
    }
}