// ============================================================
// ReactivateInventoryCommandValidator.cs
// ============================================================

using EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Inventory;

public sealed class ReactivateInventoryCommandValidator
    : AbstractValidator<ReactivateInventoryCommand>
{
    public ReactivateInventoryCommandValidator()
    {
        #region Inventory Validation

        RuleFor(x => x.InventoryId)
            .NotEmpty();

        #endregion
    }
}