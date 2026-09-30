namespace EnterpriseOrderManagementSystem.Api.Common.Models;

public sealed record ErrorResponse(
    int StatusCode,
    string Message,
    string? ErrorCode = null,
    IReadOnlyCollection<string>? Errors = null,
    string? TraceId = null);
