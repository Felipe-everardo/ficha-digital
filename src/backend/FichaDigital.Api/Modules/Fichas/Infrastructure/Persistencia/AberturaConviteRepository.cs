using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure;

internal sealed class AberturaConviteRepository(
    FichaDigitalDbContext dbContext,
    AuditoriaService auditoriaService) : IAberturaConviteRepository
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
            .SingleOrDefaultAsync(
                item => item.Id == fichaId,
                cancellationToken);
    }

    public Task<QuestionarioSaude?> ObterQuestionarioAsync(Guid fichaId, CancellationToken cancellationToken)
    {
        return dbContext.QuestionariosSaude
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == fichaId,
                cancellationToken);
    }

    public Task<DadosPessoaisFicha?> ObterDadosPessoaisAsync(Guid fichaId, CancellationToken cancellationToken)
    {
        return dbContext.DadosPessoaisFichas
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == fichaId,
                cancellationToken);
    }

    public Task<Cliente?> ObterClienteAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        return dbContext.Clientes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == clienteId,
                cancellationToken);
    }

    public async Task SalvarAberturaAsync(Ficha ficha, string correlacaoId, CancellationToken cancellationToken)
    {
        auditoriaService.AdicionarAcaoDoCliente(
            "Convite aberto",
            ficha.Id,
            correlacaoId);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
