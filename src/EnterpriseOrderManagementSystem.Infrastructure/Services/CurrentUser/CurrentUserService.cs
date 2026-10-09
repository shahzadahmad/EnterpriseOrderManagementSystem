using System.Security.Claims;
using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;

namespace EnterpriseOrderManagementSystem.Infrastructure.Services.CurrentUser;

public sealed class CurrentUserService
    : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    #region Current User

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            if (!Guid.TryParse(value, out var userId))
            {
                return null;
            }

            return userId;
        }
    }

    public string? UserName =>
        _httpContextAccessor
            .HttpContext?
            .User
            .FindFirst(ClaimTypes.Name)?
            .Value;

    public bool IsAuthenticated =>
        _httpContextAccessor
            .HttpContext?
            .User
            .Identity?
            .IsAuthenticated
        ?? false;

    #endregion
}
