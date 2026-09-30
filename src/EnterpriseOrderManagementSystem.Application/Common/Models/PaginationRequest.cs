namespace EnterpriseOrderManagementSystem.Application.Common.Models;

public sealed record PaginationRequest(
    int PageNumber = 1,
    int PageSize = 20)
{
    public int Skip =>
        (PageNumber - 1) * PageSize;
}