// ============================================================
// DeliverOrderCommandValidator.cs
// ============================================================

using EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Orders;

public sealed class DeliverOrderCommandValidator
    : AbstractValidator<DeliverOrderCommand>
{
    public DeliverOrderCommandValidator()
    {
        #region Order Validation

        RuleFor(x => x.OrderId)
            .NotEmpty();

        #endregion
    }
}