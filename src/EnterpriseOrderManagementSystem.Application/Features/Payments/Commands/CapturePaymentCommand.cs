using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed record CapturePaymentCommand(
    Guid PaymentId) : ICommand<Guid>;
