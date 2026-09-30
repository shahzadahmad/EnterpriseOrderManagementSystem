using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Inventory;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Inventory.Queries;

public sealed record GetInventoriesQuery(
    PaginationRequest Pagination)
    : IRequest<PaginatedResult<InventoryDto>>;