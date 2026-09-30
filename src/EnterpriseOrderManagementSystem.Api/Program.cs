using EnterpriseOrderManagementSystem.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();

var app = builder.Build();

// Global exception handling
app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
