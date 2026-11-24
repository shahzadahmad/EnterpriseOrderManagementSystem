
namespace EnterpriseOrderManagementSystem.Application.Common.Extensions;

public static class StringExtensions
{
    public static string Truncate(
        this string value,
        int maxLength)
    {
        ArgumentNullException.ThrowIfNull(value);

        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        return value.Length <= maxLength
            ? value
            : value[..maxLength];
    }
}
