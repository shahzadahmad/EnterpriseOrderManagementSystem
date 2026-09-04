using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;

public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }    
    public string Country { get; }
    public string PostalCode { get; }

    private Address()
    {
    }

    private Address(
        string street,
        string city,
        string state,
        string country,
        string postalCode)
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        PostalCode = postalCode;
    }

    public static Address Create(
        string street,
        string city,
        string state,
        string country,
        string postalCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(country);
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode);

        return new Address(
            street.Trim(),
            city.Trim(),
            state.Trim(),
            country.Trim(),
            postalCode.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return Country;
        yield return PostalCode;
    }

    public override string ToString()
    {
        return $"{Street}, {City}, {State}, {Country}, {PostalCode}";
    }
}