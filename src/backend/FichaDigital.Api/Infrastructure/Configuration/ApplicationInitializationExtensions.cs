using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Profissionais.Infrastructure.Provisionamento;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Infrastructure.Configuration;

internal static class ApplicationInitializationExtensions
{
    public static async Task InitializeFichaDigitalAsync(
        this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var aplicarMigrationsAoIniciar = app.Configuration.GetValue<bool>(
            "DatabaseInitialization:ApplyMigrationsOnStartup");

        if (app.Environment.IsEnvironment("E2E"))
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<FichaDigitalDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }
        else if (!app.Environment.IsEnvironment("Testing") &&
            aplicarMigrationsAoIniciar)
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<FichaDigitalDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        if (app.Environment.IsDevelopment())
        {
            var provisionadorDesenvolvimento = scope.ServiceProvider
                .GetRequiredService<ProvisionadorProfissionalDesenvolvimento>();
            await provisionadorDesenvolvimento.ProvisionarAsync();
        }

        var provisionadorInicial = scope.ServiceProvider
            .GetRequiredService<ProvisionadorProfissionalInicial>();
        await provisionadorInicial.ProvisionarAsync();
    }
}
