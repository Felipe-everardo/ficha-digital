using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure;

internal sealed class DadosPessoaisRepository(
    FichaDigitalDbContext dbContext,
    AuditoriaService auditoriaService) : IDadosPessoaisRepository
{
    public Task<ConviteFicha?> ObterConviteAsync(string tokenHash, CancellationToken cancellationToken)
    {
        return dbContext.ConvitesFicha
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.TokenHash == tokenHash,
                cancellationToken);
    }

    public Task<Ficha?> ObterFichaAsync(Guid fichaId, CancellationToken cancellationToken)
    {
        return dbContext.Fichas
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == fichaId,
                cancellationToken);
    }

    public Task<Cliente?> ObterClienteAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        return dbContext.Clientes
            .SingleOrDefaultAsync(
                item => item.Id == clienteId,
                cancellationToken);
    }

    public Task<DadosPessoaisFicha?> ObterDadosPessoaisAsync(Guid fichaId, CancellationToken cancellationToken)
    {
        return dbContext.DadosPessoaisFichas
            .SingleOrDefaultAsync(
                item => item.FichaId == fichaId,
                cancellationToken);
    }

    public async Task SalvarConfirmacaoAsync(Ficha ficha, DadosPessoaisFicha dadosDaFicha, string correlacaoId, CancellationToken cancellationToken)
    {
        if (dbContext.Entry(dadosDaFicha).State == EntityState.Detached)
        {
            dbContext.DadosPessoaisFichas.Add(dadosDaFicha);
        }

        auditoriaService.AdicionarAcaoDoCliente(
            "Dados pessoais confirmados",
            ficha.Id,
            correlacaoId);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
