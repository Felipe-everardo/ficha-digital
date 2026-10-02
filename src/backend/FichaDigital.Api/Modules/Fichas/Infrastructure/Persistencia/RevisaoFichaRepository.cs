using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure;

internal sealed class RevisaoFichaRepository(
    FichaDigitalDbContext dbContext) : IRevisaoFichaRepository
{
    public Task<Ficha?> ObterFichaAsync(Guid fichaId, CancellationToken cancellationToken)
    {
        return dbContext.Fichas.SingleOrDefaultAsync(
            item => item.Id == fichaId,
            cancellationToken);
    }

    public Task<AceiteTermoConsentimento?> ObterAceiteAsync(Guid fichaId, CancellationToken cancellationToken)
    {
        return dbContext.AceitesTermoConsentimento
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == fichaId,
                cancellationToken);
    }

    public async Task SalvarRevisaoAsync(Ficha ficha, RevisaoProfissional revisao, CancellationToken cancellationToken)
    {
        dbContext.RevisoesProfissionais.Add(revisao);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
