using EnterpriseOrderManagementSystem.Application.DTOs.Payments;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;

namespace EnterpriseOrderManagementSystem.Application.Mapping;

public static class PaymentMapping
{
    public static PaymentDto ToDto(this Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        var transactions = payment.Transactions
            .Select(transaction => new PaymentTransactionDto(
                transaction.Id,
                transaction.TransactionType,
                transaction.Amount,
                transaction.Currency,
                transaction.ProviderReference,
                transaction.Reason,
                transaction.OccurredOnUtc))
            .ToList();

        return new PaymentDto(
            payment.Id,
            payment.OrderId,
            payment.Amount,
            payment.Currency,
            payment.PaymentMethod,
            payment.PaymentProvider,
            payment.Status,
            payment.ProviderReference,
            payment.AuthorizationCode,
            payment.FailureCode,
            payment.FailureReason,
            payment.CancellationReason,
            payment.TotalRefundedAmount,
            payment.CreatedOnUtc,
            payment.AuthorizedOnUtc,
            payment.CapturedOnUtc,
            payment.SettledOnUtc,
            payment.RefundedOnUtc,
            payment.CancelledOnUtc,
            payment.FailedOnUtc,
            payment.RemainingRefundableAmount,
            payment.IsRefundable,
            payment.IsFullyRefunded,
            payment.IsTerminal,
            transactions);
    }
}