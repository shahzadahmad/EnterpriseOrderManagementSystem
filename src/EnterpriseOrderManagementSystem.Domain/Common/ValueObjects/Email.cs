using System.Text.RegularExpressions;

namespace EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;

/// <summary>
/// Represents an email address.
/// </summary>
public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        if (!EmailRegex().IsMatch(value))
        {
            throw new ArgumentException(
                "Invalid email address.",
                nameof(value));
        }

        return new Email(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    [GeneratedRegex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    public override string ToString()
    {
        return Value;
    }
}