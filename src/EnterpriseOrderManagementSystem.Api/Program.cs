using EnterpriseOrderManagementSystem.API.Middleware;
using EnterpriseOrderManagementSystem.Application.DependencyInjection;
using EnterpriseOrderManagementSystem.Infrastructure.DependencyInjection;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();

builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    #region Database Seeding

    using var scope =
        app.Services.CreateScope();

    var seeder =
        scope.ServiceProvider
            .GetRequiredService<DatabaseSeeder>();

    await seeder.SeedAsync();    

    #endregion
}

// Global exception handling
app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
