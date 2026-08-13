using FichaDigital.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace FichaDigital.IntegrationTests.Infrastructure;

public sealed class FichaDigitalApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection =
        new("Data Source=:memory:");
    private readonly TimeProvider? _timeProvider;

    public FichaDigitalApiFactory(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<FichaDigitalDbContext>>();
            services.RemoveAll<
                IDbContextOptionsConfiguration<FichaDigitalDbContext>>();
            services.RemoveAll<FichaDigitalDbContext>();
            services.RemoveAll<IDatabaseProvider>();

            services.AddDbContext<FichaDigitalDbContext>(options =>
                options.UseSqlite(_connection));

            if (_timeProvider is not null)
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton(_timeProvider);
            }
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _connection.Open();

        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();

        dbContext.Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }
}
