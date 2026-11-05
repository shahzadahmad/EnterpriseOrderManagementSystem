
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseOrderManagementSystem.Application.Behaviors;

/// <summary>
/// Executes all FluentValidation validators registered for the request
/// before the request reaches its handler.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validators = _validators.ToList();

        if (validators.Count == 0)
        {
            return await next(cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var context = new ValidationContext<TRequest>(request);

        _logger.LogInformation(
            "Starting validation for {RequestType}. Cancellation requested: {IsCancellationRequested}",
            typeof(TRequest).Name,
            cancellationToken.IsCancellationRequested);

        var failures = new List<FluentValidation.Results.ValidationFailure>();

        // Execute validators sequentially to avoid concurrent DbContext operations.
        foreach (var validator in validators)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await validator.ValidateAsync(
                context,
                cancellationToken);

            failures.AddRange(
                result.Errors.Where(error => error is not null));
        }

        if (failures.Count > 0)
        {
            _logger.LogWarning(
                "Validation failed for {RequestType}. Failure count: {FailureCount}",
                typeof(TRequest).Name,
                failures.Count);

            throw new ValidationException(failures);
        }

        _logger.LogInformation(
            "Validation completed successfully for {RequestType}.",
            typeof(TRequest).Name);

        cancellationToken.ThrowIfCancellationRequested();

        return await next(cancellationToken);
    }
}
