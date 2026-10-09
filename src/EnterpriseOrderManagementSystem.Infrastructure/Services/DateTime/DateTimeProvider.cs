using EnterpriseOrderManagementSystem.Application.Abstractions.Services;

namespace EnterpriseOrderManagementSystem.Infrastructure.Services.DateTime;

public sealed class DateTimeProvider
    : IDateTimeProvider
{
    #region Current UTC Date Time

    public System.DateTime UtcNow =>
        System.DateTime.UtcNow;

    #endregion
}