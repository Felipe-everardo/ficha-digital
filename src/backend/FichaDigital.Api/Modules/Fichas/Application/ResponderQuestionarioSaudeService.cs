using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class ResponderQuestionarioSaudeService(
    FichaDigitalDbContext dbContext,
    GeradorTokenConvite geradorToken,
    TimeProvider timeProvider)
{
    public async Task<ResultadoRespostaQuestionarioSaude> ResponderAsync(
        ResponderQuestionarioSaudeCommand command,
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
            return new ResultadoRespostaQuestionarioSaude(
                StatusRespostaQuestionarioSaude.ConviteNaoEncontrado);
        }

        if (convite.EstaExpirado(timeProvider.GetUtcNow()))
        {
            return new ResultadoRespostaQuestionarioSaude(
                StatusRespostaQuestionarioSaude.ConviteExpirado);
        }

        var ficha = await dbContext.Fichas
            .SingleOrDefaultAsync(
                item => item.Id == convite.FichaId,
                cancellationToken);

        if (ficha is null)
        {
            return new ResultadoRespostaQuestionarioSaude(
                StatusRespostaQuestionarioSaude.ConviteNaoEncontrado);
        }

        var questionarioJaExiste = await dbContext.QuestionariosSaude
            .AnyAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);

        if (questionarioJaExiste)
        {
            return new ResultadoRespostaQuestionarioSaude(
                StatusRespostaQuestionarioSaude.JaRespondido);
        }

        if (ficha.Status != StatusFicha.EmPreenchimento)
        {
            return new ResultadoRespostaQuestionarioSaude(
                StatusRespostaQuestionarioSaude.FichaIndisponivel);
        }

        var dadosPessoaisPreenchidos = await dbContext.DadosPessoaisFichas
            .AsNoTracking()
            .AnyAsync(
                dados => dados.FichaId == ficha.Id,
                cancellationToken);

        if (!dadosPessoaisPreenchidos)
        {
            return new ResultadoRespostaQuestionarioSaude(
                StatusRespostaQuestionarioSaude.DadosPessoaisPendentes);
        }

        var questionario = new QuestionarioSaude(
            ficha.Id,
            command.TemDiabetes,
            command.TipoDiabetes,
            command.TeveAnemia,
            command.DescricaoAnemia,
            command.TeveHepatite,
            command.TipoHepatite,
            command.PossuiPressaoAlta,
            command.TemAlergia,
            command.DescricaoAlergia,
            command.PossuiCondicaoCardiaca,
            command.TemEpilepsia,
            command.TemHemofilia,
            command.PossuiDoencaTransmissivel,
            command.DescricaoDoencaTransmissivel,
            command.UsaMarcaPasso,
            command.Fuma,
            command.ConsumiuBebidaAlcoolicaUltimas24Horas,
            command.UsaMedicacao,
            command.DescricaoMedicacao,
            command.EstaGravidaOuAmamentando);

        dbContext.QuestionariosSaude.Add(questionario);

        if (ficha.VersaoModelo is not null)
        {
            ficha.ConcluirAnamnese();
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ResultadoRespostaQuestionarioSaude(
            StatusRespostaQuestionarioSaude.Respondido,
            questionario.Id,
            questionario.FichaId,
            questionario.Versao,
            questionario.RespondidoEmUtc);
    }
}
