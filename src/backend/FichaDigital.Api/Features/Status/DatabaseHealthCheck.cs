using FichaDigital.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FichaDigital.Api.Features.Status;

public sealed class DatabaseHealthCheck(
    FichaDigitalDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await dbContext.Database.CanConnectAsync(cancellationToken))
                return HealthCheckResult.Unhealthy();

            if (dbContext.Database.IsSqlServer() &&
                (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
                return HealthCheckResult.Unhealthy("Existem migrations pendentes.");

            // Valida o esquema usado nas consultas, mesmo quando não há fichas.
            await dbContext.Fichas.AsNoTracking()
                .Select(ficha => ficha.VersaoConcorrencia)
                .Take(1)
                .ToListAsync(cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                exception: exception);
        }
    }
}
