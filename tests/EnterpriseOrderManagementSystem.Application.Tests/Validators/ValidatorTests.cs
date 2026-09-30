using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.Features.Customers.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Products.Commands;
using EnterpriseOrderManagementSystem.Application.Validators;
using EnterpriseOrderManagementSystem.Application.Validators.Customers;
using EnterpriseOrderManagementSystem.Application.Validators.Inventory;
using EnterpriseOrderManagementSystem.Application.Validators.Orders;
using EnterpriseOrderManagementSystem.Application.Validators.Payments;
using EnterpriseOrderManagementSystem.Application.Validators.Products;
using EnterpriseOrderManagementSystem.Domain.Enums;
using FluentValidation.TestHelper;

namespace EnterpriseOrderManagementSystem.Application.Tests.Validators;

public sealed class CustomerValidatorTests
{
    [Fact]
    public void CreateCustomer_ShouldAcceptValidCommand()
    {
        var result = new CreateCustomerCommandValidator().TestValidate(ValidCreate());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateCustomer_ShouldRejectInvalidEmail()
    {
        var result = new CreateCustomerCommandValidator().TestValidate(ValidCreate() with { Email = "invalid" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ActivateCustomer_ShouldRejectEmptyId()
    {
        new ActivateCustomerCommandValidator().TestValidate(new ActivateCustomerCommand(Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void DeactivateCustomer_ShouldRejectEmptyId()
    {
        new DeactivateCustomerCommandValidator().TestValidate(new DeactivateCustomerCommand(Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    private static CreateCustomerCommand ValidCreate() =>
        new("John", "Doe", "john@example.com", "+92", "3001234567",
            "Street", "Islamabad", "ICT", "Pakistan", "44000");
}

public sealed class InventoryValidatorTests
{
    [Fact]
    public void AdjustStock_ShouldAcceptValidCommand()
        => new AdjustStockCommandValidator().TestValidate(
            new AdjustStockCommand(Guid.NewGuid(), 0, "REF", "Reason")).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void AdjustStock_ShouldRejectNegativeQuantity()
        => new AdjustStockCommandValidator().TestValidate(
            new AdjustStockCommand(Guid.NewGuid(), -1, "REF", "Reason")).ShouldHaveValidationErrorFor(x => x.NewAvailableQuantity);

    [Fact]
    public void CommitReservation_ShouldAcceptValidCommand()
        => new CommitReservationCommandValidator().TestValidate(
            new CommitReservationCommand(Guid.NewGuid(), 1, "REF", "Reason")).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void CommitReservation_ShouldRejectZeroQuantity()
        => new CommitReservationCommandValidator().TestValidate(
            new CommitReservationCommand(Guid.NewGuid(), 0, "REF", "Reason")).ShouldHaveValidationErrorFor(x => x.Quantity);

    [Fact]
    public void DiscontinueInventory_ShouldAcceptValidCommand()
        => new DiscontinueInventoryCommandValidator().TestValidate(
            new DiscontinueInventoryCommand(Guid.NewGuid(), "Reason")).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void DiscontinueInventory_ShouldRejectBlankReason()
        => new DiscontinueInventoryCommandValidator().TestValidate(
            new DiscontinueInventoryCommand(Guid.NewGuid(), "")).ShouldHaveValidationErrorFor(x => x.Reason);

    [Fact]
    public void ReactivateInventory_ShouldRejectEmptyId()
        => new ReactivateInventoryCommandValidator().TestValidate(
            new ReactivateInventoryCommand(Guid.Empty)).ShouldHaveValidationErrorFor(x => x.InventoryId);

    [Fact]
    public void ReceiveStock_ShouldAcceptValidCommand()
        => new ReceiveStockCommandValidator().TestValidate(
            new ReceiveStockCommand(Guid.NewGuid(), 1, "REF", "Reason")).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void ReceiveStock_ShouldRejectZeroQuantity()
        => new ReceiveStockCommandValidator().TestValidate(
            new ReceiveStockCommand(Guid.NewGuid(), 0, "REF", "Reason")).ShouldHaveValidationErrorFor(x => x.Quantity);

    [Fact]
    public void ReleaseReservedStock_ShouldAcceptValidCommand()
        => new ReleaseReservedStockCommandValidator().TestValidate(
            new ReleaseReservedStockCommand(Guid.NewGuid(), 1, "REF", "Reason")).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void ReleaseReservedStock_ShouldRejectBlankReference()
        => new ReleaseReservedStockCommandValidator().TestValidate(
            new ReleaseReservedStockCommand(Guid.NewGuid(), 1, "", "Reason")).ShouldHaveValidationErrorFor(x => x.Reference);

    [Fact]
    public void ReserveStock_ShouldAcceptValidCommand()
        => new ReserveStockCommandValidator().TestValidate(
            new ReserveStockCommand(Guid.NewGuid(), 1, "REF", "Reason")).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void ReserveStock_ShouldRejectZeroQuantity()
        => new ReserveStockCommandValidator().TestValidate(
            new ReserveStockCommand(Guid.NewGuid(), 0, "REF", "Reason")).ShouldHaveValidationErrorFor(x => x.Quantity);
}

public sealed class OrderValidatorTests
{
    [Fact]
    public void AddOrderItem_ShouldAcceptValidCommand()
        => new AddOrderItemCommandValidator().TestValidate(
            new AddOrderItemCommand(Guid.NewGuid(), Guid.NewGuid(), 1)).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void AddOrderItem_ShouldRejectZeroQuantity()
        => new AddOrderItemCommandValidator().TestValidate(
            new AddOrderItemCommand(Guid.NewGuid(), Guid.NewGuid(), 0)).ShouldHaveValidationErrorFor(x => x.Quantity);

    [Fact]
    public void CancelOrder_ShouldAcceptValidCommand()
        => new CancelOrderCommandValidator().TestValidate(
            new CancelOrderCommand(Guid.NewGuid(), "Reason")).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void CancelOrder_ShouldRejectBlankReason()
        => new CancelOrderCommandValidator().TestValidate(
            new CancelOrderCommand(Guid.NewGuid(), "")).ShouldHaveValidationErrorFor(x => x.Reason);

    [Fact]
    public void ConfirmOrder_ShouldRejectEmptyId()
        => new ConfirmOrderCommandValidator().TestValidate(
            new ConfirmOrderCommand(Guid.Empty)).ShouldHaveValidationErrorFor(x => x.OrderId);

    [Fact]
    public void CreateOrder_ShouldAcceptValidCommand()
        => new CreateOrderCommandValidator().TestValidate(
            new CreateOrderCommand(Guid.NewGuid())).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void CreateOrder_ShouldRejectEmptyCustomerId()
        => new CreateOrderCommandValidator().TestValidate(
            new CreateOrderCommand(Guid.Empty)).ShouldHaveValidationErrorFor(x => x.CustomerId);

    [Fact]
    public void DeliverOrder_ShouldRejectEmptyId()
        => new DeliverOrderCommandValidator().TestValidate(
            new DeliverOrderCommand(Guid.Empty)).ShouldHaveValidationErrorFor(x => x.OrderId);

    [Fact]
    public void ShipOrder_ShouldAcceptValidCommand()
        => new ShipOrderCommandValidator().TestValidate(
            new ShipOrderCommand(Guid.NewGuid())).ShouldNotHaveAnyValidationErrors();
}

public sealed class PaymentValidatorTests
{
    [Fact]
    public void AuthorizePayment_ShouldRejectEmptyId()
        => new AuthorizePaymentCommandValidator().TestValidate(
            new AuthorizePaymentCommand(Guid.Empty)).ShouldHaveValidationErrorFor(x => x.PaymentId);

    [Fact]
    public void CancelPayment_ShouldAcceptValidCommand()
        => new CancelPaymentCommandValidator().TestValidate(
            new CancelPaymentCommand(Guid.NewGuid(), "Reason")).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void CancelPayment_ShouldRejectBlankReason()
        => new CancelPaymentCommandValidator().TestValidate(
            new CancelPaymentCommand(Guid.NewGuid(), "")).ShouldHaveValidationErrorFor(x => x.Reason);

    [Fact]
    public void CapturePayment_ShouldRejectEmptyId()
        => new CapturePaymentCommandValidator().TestValidate(
            new CapturePaymentCommand(Guid.Empty)).ShouldHaveValidationErrorFor(x => x.PaymentId);

    [Fact]
    public void CreatePayment_ShouldAcceptValidCommand()
        => new CreatePaymentCommandValidator().TestValidate(
            new CreatePaymentCommand(Guid.NewGuid(), 100.25m, "USD",
                PaymentMethod.CreditCard, PaymentProvider.Stripe)).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void CreatePayment_ShouldRejectMoreThanTwoDecimalPlaces()
        => new CreatePaymentCommandValidator().TestValidate(
            new CreatePaymentCommand(Guid.NewGuid(), 100.256m, "USD",
                PaymentMethod.CreditCard, PaymentProvider.Stripe))
            .ShouldHaveValidationErrorFor(x => x.Amount);

    [Fact]
    public void FailPayment_ShouldAcceptValidCommand()
        => new FailPaymentCommandValidator().TestValidate(
            new FailPaymentCommand(Guid.NewGuid(), "DECLINED", "Declined")).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void FailPayment_ShouldRejectBlankFailureCode()
        => new FailPaymentCommandValidator().TestValidate(
            new FailPaymentCommand(Guid.NewGuid(), "", "Declined")).ShouldHaveValidationErrorFor(x => x.FailureCode);

    [Fact]
    public void RefundPayment_ShouldAcceptValidCommand()
        => new RefundPaymentCommandValidator().TestValidate(
            new RefundPaymentCommand(Guid.NewGuid(), 10.25m, "Reason")).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void RefundPayment_ShouldRejectMoreThanTwoDecimalPlaces()
        => new RefundPaymentCommandValidator().TestValidate(
            new RefundPaymentCommand(Guid.NewGuid(), 10.256m, "Reason"))
            .ShouldHaveValidationErrorFor(x => x.RefundAmount);

    [Fact]
    public void SettlePayment_ShouldRejectEmptyId()
        => new SettlePaymentCommandValidator().TestValidate(
            new SettlePaymentCommand(Guid.Empty)).ShouldHaveValidationErrorFor(x => x.PaymentId);
}

public sealed class ProductValidatorTests
{
    [Fact]
    public void ActivateProduct_ShouldRejectEmptyId()
        => new ActivateProductCommandValidator().TestValidate(
            new ActivateProductCommand(Guid.Empty)).ShouldHaveValidationErrorFor(x => x.ProductId);

    [Fact]
    public void ChangeProductPrice_ShouldAcceptValidCommand()
        => new ChangeProductPriceCommandValidator().TestValidate(
            new ChangeProductPriceCommand(Guid.NewGuid(), 10m, "USD")).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void ChangeProductPrice_ShouldRejectNonPositivePrice()
        => new ChangeProductPriceCommandValidator().TestValidate(
            new ChangeProductPriceCommand(Guid.NewGuid(), 0m, "USD")).ShouldHaveValidationErrorFor(x => x.Price);

    [Fact]
    public void CreateProduct_ShouldAcceptValidCommand()
        => new CreateProductCommandValidator().TestValidate(
            new CreateProductCommand("SKU", "Name", "Description", 10m, "USD")).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void CreateProduct_ShouldRejectBlankSku()
        => new CreateProductCommandValidator().TestValidate(
            new CreateProductCommand("", "Name", "Description", 10m, "USD")).ShouldHaveValidationErrorFor(x => x.SKU);

    [Fact]
    public void DeactivateProduct_ShouldRejectEmptyId()
        => new DeactivateProductCommandValidator().TestValidate(
            new DeactivateProductCommand(Guid.Empty)).ShouldHaveValidationErrorFor(x => x.ProductId);

    [Fact]
    public void DiscontinueProduct_ShouldRejectEmptyId()
        => new DiscontinueProductCommandValidator().TestValidate(
            new DiscontinueProductCommand(Guid.Empty)).ShouldHaveValidationErrorFor(x => x.ProductId);

    [Fact]
    public void UpdateProduct_ShouldAcceptValidCommand()
        => new UpdateProductCommandValidator().TestValidate(
            new UpdateProductCommand(Guid.NewGuid(), "Name", "Description")).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void UpdateProduct_ShouldRejectBlankName()
        => new UpdateProductCommandValidator().TestValidate(
            new UpdateProductCommand(Guid.NewGuid(), "", "Description")).ShouldHaveValidationErrorFor(x => x.Name);
}

public sealed class PaginationValidatorTests
{
    [Fact]
    public void Pagination_ShouldAcceptValidRequest()
        => new PaginationRequestValidator().TestValidate(new PaginationRequest(1, 20))
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Pagination_ShouldRejectInvalidPageNumber()
        => new PaginationRequestValidator().TestValidate(new PaginationRequest(0, 20))
            .ShouldHaveValidationErrorFor(x => x.PageNumber);

    [Fact]
    public void Pagination_ShouldRejectInvalidPageSize()
        => new PaginationRequestValidator().TestValidate(new PaginationRequest(1, 101))
            .ShouldHaveValidationErrorFor(x => x.PageSize);
}
