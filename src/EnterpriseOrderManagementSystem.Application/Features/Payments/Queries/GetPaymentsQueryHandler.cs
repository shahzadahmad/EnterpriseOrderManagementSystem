using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Payments;
using EnterpriseOrderManagementSystem.Application.Mapping;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Queries;

public sealed class GetPaymentsQueryHandler
    : IRequestHandler<GetPaymentsQuery, PaginatedResult<PaymentDto>>
{
    #region Fields

    private readonly IReadRepository<Payment> _readRepository;

    #endregion

    #region Constructor

    public GetPaymentsQueryHandler(
        IReadRepository<Payment> readRepository)
    {
        _readRepository = readRepository;
    }

    #endregion

    #region Handle

    public async Task<PaginatedResult<PaymentDto>> Handle(
        GetPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;

        var totalCount = await _readRepository.CountAsync(
            cancellationToken);

        var payments = await _readRepository.ListAsync(
            pagination.Skip,
            pagination.PageSize,
            cancellationToken);

        var items = payments
            .Select(payment => payment.ToDto())
            .ToArray();

        return new PaginatedResult<PaymentDto>(
            items,
            pagination.PageNumber,
            pagination.PageSize,
            totalCount);
    }

    #endregion
}