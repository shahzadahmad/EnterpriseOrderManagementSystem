// ============================================================
// ShipOrderCommandValidator.cs
// ============================================================

using EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Orders;

public sealed class ShipOrderCommandValidator
    : AbstractValidator<ShipOrderCommand>
{
    public ShipOrderCommandValidator()
    {
        #region Order Validation

        RuleFor(x => x.OrderId)
            .NotEmpty();

        #endregion
    }
}