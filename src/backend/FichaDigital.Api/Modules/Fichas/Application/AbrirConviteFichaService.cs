using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class AbrirConviteFichaService(
    FichaDigitalDbContext dbContext,
    GeradorTokenConvite geradorToken,
    TimeProvider timeProvider)
{
    public async Task<ResultadoAberturaConvite> AbrirAsync(
        string tokenOriginal,
        CancellationToken cancellationToken)
    {
        var tokenHash = geradorToken.CalcularHash(tokenOriginal);

        var convite = await dbContext.ConvitesFicha
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.TokenHash == tokenHash,
                cancellationToken);

        if (convite is null)
        {
            return new ResultadoAberturaConvite(
                StatusAberturaConvite.NaoEncontrado);
        }

        if (convite.EstaExpirado(timeProvider.GetUtcNow()))
        {
            return new ResultadoAberturaConvite(
                StatusAberturaConvite.Expirado);
        }

        var ficha = await dbContext.Fichas
            .SingleOrDefaultAsync(
                item => item.Id == convite.FichaId,
                cancellationToken);

        if (ficha is null)
        {
            return new ResultadoAberturaConvite(
                StatusAberturaConvite.NaoEncontrado);
        }

        if (ficha.Status == StatusFicha.ConviteEnviado)
        {
            ficha.IniciarPreenchimento();
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (ficha.Status != StatusFicha.EmPreenchimento)
        {
            return new ResultadoAberturaConvite(
                StatusAberturaConvite.Indisponivel);
        }

        var questionarioRespondido = await dbContext.QuestionariosSaude
            .AsNoTracking()
            .AnyAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);

        return new ResultadoAberturaConvite(
            StatusAberturaConvite.Aberto,
            ficha.Id,
            ficha.Status,
            questionarioRespondido);
    }
}
