namespace FichaDigital.Api.Infrastructure.Auditing;

public sealed class AuditoriaService(
    Persistence.FichaDigitalDbContext dbContext,
    TimeProvider timeProvider)
{
    public void AdicionarAcaoDoCliente(
        string acao,
        Guid fichaId,
        string correlacaoId)
    {
        dbContext.RegistrosAuditoria.Add(
            RegistroAuditoria.CriarParaCliente(
                acao,
                fichaId,
                correlacaoId,
                timeProvider.GetUtcNow()));
    }
}
