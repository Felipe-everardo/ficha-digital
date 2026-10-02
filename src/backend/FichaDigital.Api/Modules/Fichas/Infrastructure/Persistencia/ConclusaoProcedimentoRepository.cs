using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure;

internal sealed class ConclusaoProcedimentoRepository(
    FichaDigitalDbContext dbContext) : IConclusaoProcedimentoRepository
{
    public Task<Ficha?> ObterFichaAsync(Guid fichaId, CancellationToken cancellationToken)
    {
        return dbContext.Fichas.SingleOrDefaultAsync(
            item => item.Id == fichaId,
            cancellationToken);
    }

    public async Task SalvarConclusaoAsync(Ficha ficha, RegistroTatuagem? tatuagem, RegistroPiercing? piercing, CancellationToken cancellationToken)
    {
        if (tatuagem is not null)
        {
            dbContext.RegistrosTatuagem.Add(tatuagem);
        }

        if (piercing is not null)
        {
            dbContext.RegistrosPiercing.Add(piercing);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
