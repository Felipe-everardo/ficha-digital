using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Infrastructure.Idempotency;

public sealed class LimpezaIdempotenciaService(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<LimpezaIdempotenciaService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(6));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await LimparAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Falha ao remover registros de idempotência expirados.");
            }

        }
    }

    private async Task LimparAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<
            Persistence.FichaDigitalDbContext>();
        var limite = timeProvider.GetUtcNow();
        var removidos = await dbContext.RequisicoesIdempotentes
            .Where(requisicao => requisicao.ExpiraEmUtc <= limite)
            .ExecuteDeleteAsync(cancellationToken);

        if (removidos > 0)
        {
            logger.LogInformation(
                "Foram removidos {Quantidade} registros de idempotência expirados.",
                removidos);
        }
    }
}
