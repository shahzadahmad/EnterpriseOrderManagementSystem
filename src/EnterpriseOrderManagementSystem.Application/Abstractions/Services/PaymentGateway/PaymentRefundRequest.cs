// PaymentRefundRequest.cs

namespace EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;

public sealed record PaymentRefundRequest(
    Guid PaymentId,
    string ProviderReference,
    decimal Amount,
    string Currency);