using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Application.DTOs.Orders;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Queries;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto?>;