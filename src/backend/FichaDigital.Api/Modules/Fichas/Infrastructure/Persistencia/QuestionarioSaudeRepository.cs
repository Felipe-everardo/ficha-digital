using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure;

internal sealed class QuestionarioSaudeRepository(
    FichaDigitalDbContext dbContext,
    AuditoriaService auditoriaService) : IQuestionarioSaudeRepository
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

    public Task<bool> QuestionarioExisteAsync(Guid fichaId, CancellationToken cancellationToken)
    {
        return dbContext.QuestionariosSaude
            .AnyAsync(
                item => item.FichaId == fichaId,
                cancellationToken);
    }

    public Task<bool> DadosPessoaisExistemAsync(Guid fichaId, CancellationToken cancellationToken)
    {
        return dbContext.DadosPessoaisFichas
            .AsNoTracking()
            .AnyAsync(
                dados => dados.FichaId == fichaId,
                cancellationToken);
    }

    public async Task SalvarRespostaAsync(Ficha ficha, QuestionarioSaude questionario, string correlacaoId, CancellationToken cancellationToken)
    {
        dbContext.QuestionariosSaude.Add(questionario);
        auditoriaService.AdicionarAcaoDoCliente(
            "Questionário de saúde respondido",
            ficha.Id,
            correlacaoId);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
