using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed record AuthorizePaymentCommand(
    Guid PaymentId) : ICommand<Guid>;