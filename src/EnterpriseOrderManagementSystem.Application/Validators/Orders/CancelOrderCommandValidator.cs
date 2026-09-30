// ============================================================
// CancelOrderCommandValidator.cs
// ============================================================

using EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Orders;

public sealed class CancelOrderCommandValidator
    : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        #region Order Validation

        RuleFor(x => x.OrderId)
            .NotEmpty();

        #endregion

        #region Reason Validation

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);

        #endregion
    }
}