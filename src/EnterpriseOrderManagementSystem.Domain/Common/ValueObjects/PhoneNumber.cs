using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    public string CountryCode { get; }

    public string NationalNumber { get; }

    private PhoneNumber(
        string countryCode,
        string nationalNumber)
    {
        CountryCode = countryCode;
        NationalNumber = nationalNumber;
    }

    public static PhoneNumber Create(
        string countryCode,
        string nationalNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(nationalNumber);

        countryCode = countryCode.Trim();
        nationalNumber = nationalNumber.Trim();

        if (!countryCode.StartsWith('+'))
            throw new ArgumentException(
                "Country code must start with '+'.",
                nameof(countryCode));

        if (!countryCode[1..].All(char.IsDigit))
            throw new ArgumentException(
                "Country code must contain only digits after '+'.",
                nameof(countryCode));

        if (!nationalNumber.All(char.IsDigit))
            throw new ArgumentException(
                "National number must contain only digits.",
                nameof(nationalNumber));

        return new PhoneNumber(
            countryCode,
            nationalNumber);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return NationalNumber;
    }

    public override string ToString()
    {
        return $"{CountryCode} {NationalNumber}";
    }
}