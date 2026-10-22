using EnterpriseOrderManagementSystem.API.Middleware;
using EnterpriseOrderManagementSystem.Application.DependencyInjection;
using EnterpriseOrderManagementSystem.Infrastructure.DependencyInjection;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Seeding;

var builder = WebApplication.CreateBuilder(args);

#region Service Registration

builder.Services.AddControllers();

// Swagger / OpenAPI services for interactive API testing.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Enterprise Order Management System API",
        Version = "v1",
        Description = "API endpoints for the Enterprise Order Management System."
    });
});


builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

#endregion

var app = builder.Build();

#region HTTP Request Pipeline

// Enable Swagger UI in Development so endpoints can be tested interactively.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Enterprise Order Management System API v1");
        options.RoutePrefix = "swagger";
    });


    #region Database Seeding

    using var scope =
        app.Services.CreateScope();

    var seeder =
        scope.ServiceProvider
            .GetRequiredService<DatabaseSeeder>();

    await seeder.SeedAsync();    

    #endregion
}

// Convert downstream exceptions into the API's error response format.
app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();


// Allows WebApplicationFactory to bootstrap the API in integration tests.
public partial class Program
{
}