using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseOrderManagementSystem.Application.Behaviors;

/// <summary>
/// Logs the start, successful completion, and failure of MediatR requests.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation(
            "Handling request {RequestName}",
            requestName);

        try
        {
            var response = await next(cancellationToken);

            _logger.LogInformation(
                "Successfully handled request {RequestName}",
                requestName);

            return response;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error handling request {RequestName}",
                requestName);

            throw;
        }
    }
}