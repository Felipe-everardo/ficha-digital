using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Profissionais.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace FichaDigital.IntegrationTests.Infrastructure.SqlServer;

public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    private const string SqlServerImage =
        "mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04";

    private MsSqlContainer? _container;
    private string? _connectionString;
    private ServiceProvider? _serviceProvider;

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

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<FichaDigitalDbContext>(options =>
            options.UseSqlServer(_connectionString));
        services
            .AddIdentityCore<ProfissionalUsuario>(options =>
                options.User.RequireUniqueEmail = true)
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<FichaDigitalDbContext>();
        _serviceProvider = services.BuildServiceProvider(
            validateScopes: true);

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

    public IServiceScope CreateServiceScope()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException(
                "Os serviços do teste ainda não foram inicializados.");
        }

        return _serviceProvider.CreateScope();
    }

    public async ValueTask DisposeAsync()
    {
        if (_serviceProvider is not null)
        {
            await _serviceProvider.DisposeAsync();
        }

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
