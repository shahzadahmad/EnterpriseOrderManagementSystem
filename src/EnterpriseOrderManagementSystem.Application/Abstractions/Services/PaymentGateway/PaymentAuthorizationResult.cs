// PaymentAuthorizationResult.cs

namespace EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;

public sealed record PaymentAuthorizationResult(
    bool IsSuccessful,
    string? ProviderReference,
    string? AuthorizationCode,
    string? FailureCode,
    string? FailureReason);