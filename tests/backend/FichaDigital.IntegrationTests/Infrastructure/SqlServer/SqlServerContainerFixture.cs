using FichaDigital.Api.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace FichaDigital.IntegrationTests.Infrastructure.SqlServer;

public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    private const string SqlServerImage =
        "mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04";

    private MsSqlContainer? _container;
    private string? _connectionString;

    public async ValueTask InitializeAsync()
    {
        if (!SqlServerTestEnvironment.Enabled)
        {
            return;
        }

        _container = new MsSqlBuilder(SqlServerImage).Build();
        await _container.StartAsync();

        var connectionStringBuilder = new SqlConnectionStringBuilder(
            _container.GetConnectionString())
        {
            InitialCatalog = $"FichaDigitalTests_{Guid.NewGuid():N}"
        };
        _connectionString = connectionStringBuilder.ConnectionString;

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public FichaDigitalDbContext CreateDbContext()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                "O container SQL Server ainda não foi inicializado.");
        }

        var options = new DbContextOptionsBuilder<FichaDigitalDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        return new FichaDigitalDbContext(options);
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
