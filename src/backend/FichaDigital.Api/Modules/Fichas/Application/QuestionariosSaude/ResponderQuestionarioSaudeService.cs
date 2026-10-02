using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class ResponderQuestionarioSaudeService(
    IQuestionarioSaudeRepository repository,
    IGeradorTokenConvite geradorToken,
    TimeProvider timeProvider)
{
    public async Task<ResultadoRespostaQuestionarioSaude> ResponderAsync(
        ResponderQuestionarioSaudeCommand command,
        string correlacaoId,
        CancellationToken cancellationToken)
    {
        var tokenHash = geradorToken.CalcularHash(command.TokenOriginal);

        var convite = await repository.ObterConviteAsync(tokenHash, cancellationToken);

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

        var ficha = await repository.ObterFichaAsync(convite.FichaId, cancellationToken);

        if (ficha is null)
        {
            return new ResultadoRespostaQuestionarioSaude(
                StatusRespostaQuestionarioSaude.ConviteNaoEncontrado);
        }

        var questionarioJaExiste = await repository.QuestionarioExisteAsync(ficha.Id, cancellationToken);

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

        var dadosPessoaisPreenchidos = await repository.DadosPessoaisExistemAsync(ficha.Id, cancellationToken);

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

        if (ficha.VersaoModelo is not null)
        {
            ficha.ConcluirAnamnese();
        }

        await repository.SalvarRespostaAsync(ficha, questionario, correlacaoId, cancellationToken);

        return new ResultadoRespostaQuestionarioSaude(
            StatusRespostaQuestionarioSaude.Respondido,
            questionario.Id,
            questionario.FichaId,
            questionario.Versao,
            questionario.RespondidoEmUtc);
    }
}
