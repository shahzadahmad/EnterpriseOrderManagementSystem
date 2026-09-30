using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed record SettlePaymentCommand(
    Guid PaymentId) : ICommand<Guid>;