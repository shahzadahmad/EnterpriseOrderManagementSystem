using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;
using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Context;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Repositories;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Seeding;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Transactions;
using EnterpriseOrderManagementSystem.Infrastructure.Services.CurrentUser;
using EnterpriseOrderManagementSystem.Infrastructure.Services.DateTime;
using EnterpriseOrderManagementSystem.Infrastructure.Services.DomainEvents;
using EnterpriseOrderManagementSystem.Infrastructure.Services.Email;
using EnterpriseOrderManagementSystem.Infrastructure.Services.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseOrderManagementSystem.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        #region Database

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString(
                    "DefaultConnection"));
        });

        #endregion

        #region Write Repositories

        services.AddScoped<
            ICustomerRepository,
            CustomerRepository>();

        services.AddScoped<
            IProductRepository,
            ProductRepository>();

        services.AddScoped<
            IOrderRepository,
            OrderRepository>();

        services.AddScoped<
            IInventoryRepository,
            InventoryRepository>();

        services.AddScoped<
            IPaymentRepository,
            PaymentRepository>();

        #endregion

        #region Read Repository

        services.AddScoped(
            typeof(IReadRepository<>),
            typeof(EfReadRepository<>));

        #endregion

        #region Unit Of Work

        services.AddScoped<
            IUnitOfWork,
            UnitOfWork>();

        #endregion

        #region Domain Events

        services.AddScoped<
            IDomainEventDispatcher,
            DomainEventDispatcher>();

        #endregion

        #region Current User

        services.AddHttpContextAccessor();

        services.AddScoped<
            ICurrentUserService,
            CurrentUserService>();

        #endregion

        #region Date Time

        services.AddSingleton<
            IDateTimeProvider,
            DateTimeProvider>();

        #endregion

        #region Email

        services.Configure<EmailOptions>(
            configuration.GetSection("Email"));

        services.AddScoped<
            IEmailService,
            SmtpEmailService>();

        #endregion

        #region Payment Gateway

        services.AddScoped<
            IPaymentGateway,
            MockPaymentGateway>();

        #endregion

        #region Database Seeding

        services.AddScoped<
            DatabaseSeeder>();

        #endregion

        return services;
    }
}