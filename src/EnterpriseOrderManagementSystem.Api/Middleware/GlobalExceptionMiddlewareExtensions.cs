using EnterpriseOrderManagementSystem.Api.Middleware;

namespace EnterpriseOrderManagementSystem.API.Middleware;

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}