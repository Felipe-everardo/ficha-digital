namespace FichaDigital.Api.Infrastructure.Auditing;

public sealed class AuditoriaService(
    Persistence.FichaDigitalDbContext dbContext,
    TimeProvider timeProvider)
{
    public async Task RegistrarAcaoDoClienteAsync(
        string acao,
        Guid fichaId,
        string correlacaoId,
        CancellationToken cancellationToken)
    {
        dbContext.RegistrosAuditoria.Add(
            RegistroAuditoria.CriarParaCliente(
                acao,
                fichaId,
                correlacaoId,
                timeProvider.GetUtcNow()));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
