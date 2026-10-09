using EnterpriseOrderManagementSystem.Infrastructure.Tests.Database;
using Xunit;

namespace EnterpriseOrderManagementSystem.Infrastructure.Tests.Database;

[CollectionDefinition("SQL Server")]
public sealed class SqlServerCollection
    : ICollectionFixture<SqlServerFixture>
{
}