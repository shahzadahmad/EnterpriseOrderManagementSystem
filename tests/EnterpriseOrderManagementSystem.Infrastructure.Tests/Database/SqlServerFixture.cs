using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace EnterpriseOrderManagementSystem.Infrastructure.Tests.Database
{
    public sealed class SqlServerFixture
     : IAsyncLifetime
    {
        private readonly MsSqlContainer _container =
            new MsSqlBuilder(
                "mcr.microsoft.com/mssql/server:2022-latest")
                .Build();

        public string ConnectionString =>
            _container.GetConnectionString();

        public async Task InitializeAsync()
        {
            using var timeout =
                new CancellationTokenSource(
                    TimeSpan.FromMinutes(3));

            await _container.StartAsync(
                timeout.Token);

            await using var context =
                CreateDbContext();

            await context.Database.MigrateAsync(
                timeout.Token);
        }

        public async Task DisposeAsync()
        {
            await _container.DisposeAsync();
        }

        public AppDbContext CreateDbContext()
        {
            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(ConnectionString)
                    .Options;

            return new AppDbContext(options);
        }
    }
}
