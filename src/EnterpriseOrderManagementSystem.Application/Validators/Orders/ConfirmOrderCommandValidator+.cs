// ============================================================
// ConfirmOrderCommandValidator.cs
// ============================================================

using EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Orders;

public sealed class ConfirmOrderCommandValidator
    : AbstractValidator<ConfirmOrderCommand>
{
    public ConfirmOrderCommandValidator()
    {
        #region Order Validation

        RuleFor(x => x.OrderId)
            .NotEmpty();

        #endregion
    }
}