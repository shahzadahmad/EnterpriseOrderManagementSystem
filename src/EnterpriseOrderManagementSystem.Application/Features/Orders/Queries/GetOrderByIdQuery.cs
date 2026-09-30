using EnterpriseOrderManagementSystem.Application.DTOs.Orders;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Queries;

public sealed record GetOrderByIdQuery(
    Guid OrderId) : IRequest<OrderDto?>;