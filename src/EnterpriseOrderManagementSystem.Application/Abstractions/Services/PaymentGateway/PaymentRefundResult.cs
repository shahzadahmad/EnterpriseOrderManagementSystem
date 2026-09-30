// PaymentRefundResult.cs

namespace EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;

public sealed record PaymentRefundResult(
    bool IsSuccessful,
    string? ProviderReference,
    string? FailureCode,
    string? FailureReason);