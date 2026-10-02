using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class RevisarFichaService(
    IRevisaoFichaRepository repository,
    ICalculadorHashConteudo calculadorHash,
    TimeProvider timeProvider)
{
    public async Task<ResultadoRevisaoFicha> RevisarAsync(
        Guid fichaId,
        Guid profissionalId,
        bool dadosDaFichaConferidos,
        CancellationToken cancellationToken)
    {
        var ficha = await repository.ObterFichaAsync(fichaId, cancellationToken);

        if (ficha is null)
        {
            return new ResultadoRevisaoFicha(
                StatusRevisaoFicha.FichaNaoEncontrada);
        }

        if (ficha.ProfissionalResponsavelId != profissionalId)
        {
            return new ResultadoRevisaoFicha(
                StatusRevisaoFicha.ProfissionalNaoResponsavel);
        }

        if (ficha.Status != StatusFicha.AutorizadaParaProcedimento)
        {
            return new ResultadoRevisaoFicha(
                StatusRevisaoFicha.FichaIndisponivel);
        }

        if (!dadosDaFichaConferidos)
        {
            return new ResultadoRevisaoFicha(
                StatusRevisaoFicha.DadosNaoConferidos);
        }

        var aceite = await repository.ObterAceiteAsync(ficha.Id, cancellationToken);

        if (aceite is null ||
            aceite.AssinaturaDesenhada is null ||
            !aceite.ConfirmouLeituraEAutorizacao ||
            !string.Equals(
                calculadorHash.Calcular(aceite.EvidenciaJson),
                aceite.EvidenciaHash,
                StringComparison.OrdinalIgnoreCase))
        {
            return new ResultadoRevisaoFicha(
                StatusRevisaoFicha.ConsentimentoInvalido);
        }

        var revisao = new RevisaoProfissional(
            ficha.Id,
            profissionalId,
            ficha.ProfissionalResponsavelNome,
            dadosDaFichaConferidos,
            timeProvider.GetUtcNow());

        ficha.ConfirmarRevisaoProfissional();
        await repository.SalvarRevisaoAsync(ficha, revisao, cancellationToken);

        return new ResultadoRevisaoFicha(
            StatusRevisaoFicha.Confirmada,
            ficha.Id,
            ficha.Status,
            revisao.RevisadaEmUtc);
    }
}
