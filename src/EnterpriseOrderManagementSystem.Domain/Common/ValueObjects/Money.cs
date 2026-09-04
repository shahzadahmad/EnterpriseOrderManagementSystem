namespace EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }

    public string Currency { get; }

    private Money(
        decimal amount,
        string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(
        decimal amount,
        string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new Money(
            amount,
            currency.ToUpperInvariant());
    }

    /// <summary>
    /// Creates a zero-valued money instance for the specified currency.
    /// </summary>
    public static Money Zero(string currency)
    {
        return Create(0m, currency);
    }

    /// <summary>
    /// Adds two money values of the same currency.
    /// </summary>
    public static Money operator +(Money left, Money right)
    {
        ValidateSameCurrency(left, right);

        return new Money(
            left.Amount + right.Amount,
            left.Currency);
    }

    /// <summary>
    /// Subtracts two money values of the same currency.
    /// </summary>
    public static Money operator -(Money left, Money right)
    {
        ValidateSameCurrency(left, right);

        var amount = left.Amount - right.Amount;

        if (amount < 0)
            throw new InvalidOperationException(
                "Money amount cannot be negative.");

        return new Money(
            amount,
            left.Currency);
    }

    /// <summary>
    /// Multiplies a money value by a quantity.
    /// </summary>
    public static Money operator *(Money money, int quantity)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        return new Money(
            money.Amount * quantity,
            money.Currency);
    }

    private static void ValidateSameCurrency(
        Money left,
        Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (!string.Equals(
                left.Currency,
                right.Currency,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Money operations require both values to use the same currency.");
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString()
    {
        return $"{Amount} {Currency}";
    }
}