using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;

public sealed class AddOrderItemCommandHandler
    : IRequestHandler<AddOrderItemCommand, Guid>
{
    #region Fields

    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    #endregion

    #region Constructor

    public AddOrderItemCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        AddOrderItemCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(
            request.OrderId,
            cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(
                "Order",
                request.OrderId);
        }

        var product = await _productRepository.GetByIdAsync(
            request.ProductId,
            cancellationToken);

        if (product is null)
        {
            throw new NotFoundException(
                "Product",
                request.ProductId);
        }

        var unitPrice = Money.Create(
            product.Price.Amount,
            product.Price.Currency);

        order.AddItem(
            product.Id,
            product.Name,
            unitPrice,
            request.Quantity);

        return order.Id;
    }

    #endregion
}