// PaymentAuthorizationRequest.cs

namespace EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;

public sealed record PaymentAuthorizationRequest(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string PaymentMethod);