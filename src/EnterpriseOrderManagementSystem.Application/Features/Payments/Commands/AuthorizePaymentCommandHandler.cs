using EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed class AuthorizePaymentCommandHandler
    : IRequestHandler<AuthorizePaymentCommand, Guid>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;

    public AuthorizePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway)
    {
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
    }

    public async Task<Guid> Handle(
        AuthorizePaymentCommand request,
        CancellationToken cancellationToken)
    {
        #region Load Payment

        var payment = await _paymentRepository.GetByIdAsync(
            request.PaymentId,
            cancellationToken);

        if (payment is null)
        {
            throw new NotFoundException(
                "Payment",
                request.PaymentId);
        }

        #endregion

        #region Authorize Through Gateway

        var gatewayResult = await _paymentGateway.AuthorizeAsync(
            new PaymentAuthorizationRequest(
                payment.Id,
                payment.OrderId,
                payment.Amount,
                payment.Currency,
                payment.PaymentMethod.ToString()),
            cancellationToken);

        #endregion

        #region Handle Gateway Result

        if (!gatewayResult.IsSuccessful)
        {
            payment.Fail(
                gatewayResult.FailureCode ?? "PAYMENT_AUTHORIZATION_FAILED",
                gatewayResult.FailureReason ?? "Payment authorization failed.");

            return payment.Id;
        }

        #endregion

        #region Update Domain

        payment.Authorize(
            gatewayResult.ProviderReference!,
            gatewayResult.AuthorizationCode!);

        #endregion

        return payment.Id;
    }
}