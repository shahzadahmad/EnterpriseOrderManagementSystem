// PaymentCaptureRequest.cs

namespace EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;

public sealed record PaymentCaptureRequest(
    Guid PaymentId,
    string ProviderReference,
    decimal Amount,
    string Currency);