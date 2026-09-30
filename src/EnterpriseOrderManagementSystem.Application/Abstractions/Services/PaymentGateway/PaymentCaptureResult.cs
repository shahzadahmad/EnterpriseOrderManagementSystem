// PaymentCaptureResult.cs

namespace EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;

public sealed record PaymentCaptureResult(
    bool IsSuccessful,
    string? ProviderReference,
    string? FailureCode,
    string? FailureReason);