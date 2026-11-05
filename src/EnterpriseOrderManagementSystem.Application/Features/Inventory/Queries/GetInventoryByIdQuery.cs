using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Application.DTOs.Inventory;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Inventory.Queries;

public sealed record GetInventoryByIdQuery(Guid InventoryId) : IQuery<InventoryDto?>;
