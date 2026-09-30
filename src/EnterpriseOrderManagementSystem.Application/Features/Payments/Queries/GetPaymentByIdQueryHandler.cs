using EnterpriseOrderManagementSystem.Application.DTOs.Payments;
using EnterpriseOrderManagementSystem.Application.Mapping;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Queries;

public sealed class GetPaymentByIdQueryHandler
    : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{
    #region Fields

    private readonly IPaymentRepository _paymentRepository;

    #endregion

    #region Constructor

    public GetPaymentByIdQueryHandler(
        IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    #endregion

    #region Handle

    public async Task<PaymentDto?> Handle(
        GetPaymentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(
            request.PaymentId,
            cancellationToken);

        return payment?.ToDto();
    }

    #endregion
}
