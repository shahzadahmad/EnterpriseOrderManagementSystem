using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed record CancelPaymentCommand(
    Guid PaymentId,
    string Reason) : ICommand<Guid>;