using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class AceitarTermoConsentimentoService(
    FichaDigitalDbContext dbContext,
    GeradorTokenConvite geradorToken,
    CalculadorHashConteudo calculadorHash,
    TimeProvider timeProvider)
{
    public async Task<ResultadoAceiteTermoConsentimento> AceitarAsync(
        AceitarTermoConsentimentoCommand command,
        CancellationToken cancellationToken)
    {
        var tokenHash = geradorToken.CalcularHash(command.TokenOriginal);
        var convite = await dbContext.ConvitesFicha
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.TokenHash == tokenHash,
                cancellationToken);

        if (convite is null)
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.ConviteNaoEncontrado);
        }

        if (convite.EstaExpirado(timeProvider.GetUtcNow()))
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.ConviteExpirado);
        }

        var ficha = await dbContext.Fichas
            .SingleOrDefaultAsync(
                item => item.Id == convite.FichaId,
                cancellationToken);

        if (ficha is null)
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.ConviteNaoEncontrado);
        }

        if (ficha.Status != StatusFicha.EmPreenchimento)
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.FichaIndisponivel);
        }

        var questionarioExiste = await dbContext.QuestionariosSaude
            .AnyAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);

        if (!questionarioExiste)
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.QuestionarioPendente);
        }

        var aceiteJaExiste = await dbContext.AceitesTermoConsentimento
            .AnyAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);

        if (aceiteJaExiste)
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.JaAceito);
        }

        var conteudoHashAtual = calculadorHash.Calcular(
            TermoConsentimentoAtual.Conteudo);

        if (command.VersaoTermo != TermoConsentimentoAtual.Versao ||
            !string.Equals(
                command.ConteudoHash,
                conteudoHashAtual,
                StringComparison.OrdinalIgnoreCase))
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.TermoDesatualizado);
        }

        var aceite = new AceiteTermoConsentimento(
            ficha.Id,
            TermoConsentimentoAtual.Versao,
            TermoConsentimentoAtual.Conteudo,
            conteudoHashAtual,
            command.NomeAssinante,
            timeProvider.GetUtcNow());

        ficha.Concluir();
        dbContext.AceitesTermoConsentimento.Add(aceite);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ResultadoAceiteTermoConsentimento(
            StatusAceiteTermoConsentimento.Aceito,
            aceite.Id,
            aceite.FichaId,
            aceite.VersaoTermo,
            aceite.AceitoEmUtc);
    }
}
