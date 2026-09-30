using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Application.Tests.Common;

public static class TestData
{
    public static Customer Customer() =>
            Domain.Aggregates.CustomerAggregate.Customer.Create("John", "Doe", Email.Create("john.doe@example.com"),
            PhoneNumber.Create("+92", "3001234567"),
            Address.Create("1 Main Street", "Islamabad", "ICT", "Pakistan", "44000"));

    public static Product Product(decimal price = 100m) =>
        Domain.Aggregates.ProductAggregate.Product.Create("SKU-001", "Test Product", "Test description", Money.Create(price, "USD"));

    public static Inventory Inventory(int maximumQuantity = 100) =>
        Domain.Aggregates.InventoryAggregate.Inventory.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10, maximumQuantity);

    public static Order Order() => Domain.Aggregates.OrderAggregate.Order.Create(Guid.NewGuid());

    public static Payment Payment(decimal amount = 100m) =>
        Domain.Aggregates.PaymentAggregate.Payment.Create(Guid.NewGuid(), Guid.NewGuid(), amount, "USD",
            PaymentMethod.CreditCard, PaymentProvider.Stripe);

    public static Order OrderWithItem(decimal price = 100m, int quantity = 2)
    {
        var order = Order();
        var product = Product(price);
        order.AddItem(product.Id, product.Name, product.Price, quantity);
        return order;
    }

    public static Order ConfirmedOrder()
    {
        var order = OrderWithItem();
        order.Confirm();
        return order;
    }

    public static Order ShippedOrder()
    {
        var order = ConfirmedOrder();
        order.Ship();
        return order;
    }

    public static Payment AuthorizedPayment()
    {
        var payment = Payment();
        payment.Authorize("auth-ref", "auth-code");
        return payment;
    }

    public static Payment CapturedPayment()
    {
        var payment = AuthorizedPayment();
        payment.Capture("capture-ref");
        return payment;
    }

    public static Payment SettledPayment()
    {
        var payment = CapturedPayment();
        payment.Settle("settlement-ref");
        return payment;
    }

    public static Inventory InventoryWithStock(int quantity = 10)
    {
        var inventory = Inventory();
        inventory.ReceiveStock(quantity, "PO-001", "Initial stock");
        return inventory;
    }

    public static Inventory InventoryWithReservation(int available = 10, int reserved = 3)
    {
        var inventory = InventoryWithStock(available);
        inventory.ReserveStock(reserved, "ORDER-001", "Reservation");
        return inventory;
    }
}
