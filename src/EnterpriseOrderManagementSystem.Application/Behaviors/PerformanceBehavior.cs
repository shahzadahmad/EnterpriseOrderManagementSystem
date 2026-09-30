using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EnterpriseOrderManagementSystem.Application.Behaviors;

/// <summary>
/// Measures the execution time of each MediatR request.
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            return await next(cancellationToken);
        }
        finally
        {
            stopwatch.Stop();

            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds >= 500)
            {
                _logger.LogWarning(
                    "Long-running request detected. Request: {RequestName}, " +
                    "ElapsedMilliseconds: {ElapsedMilliseconds}",
                    typeof(TRequest).Name,
                    elapsedMilliseconds);
            }
            else
            {
                _logger.LogDebug(
                    "Request {RequestName} completed in {ElapsedMilliseconds} ms",
                    typeof(TRequest).Name,
                    elapsedMilliseconds);
            }
        }
    }
}
