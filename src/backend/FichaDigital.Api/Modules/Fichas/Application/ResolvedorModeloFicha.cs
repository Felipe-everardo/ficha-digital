using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure;
using Microsoft.Extensions.Options;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class ResolvedorModeloFicha(IOptions<EstudioOptions> options)
{
    private readonly string _cnpj = options.Value.Cnpj.Trim();

    public ModeloFichaSelecionado ObterModeloAtual(
        TipoProcedimento tipoProcedimento)
    {
        return tipoProcedimento switch
        {
            TipoProcedimento.Tatuagem => CriarModelo(
                TipoProcedimento.Tatuagem,
                TermosProcedimento.VersaoTermoTatuagemAtual,
                _cnpj),
            TipoProcedimento.Piercing => CriarModelo(
                TipoProcedimento.Piercing,
                TermosProcedimento.VersaoTermoPiercingAtual,
                _cnpj),
            _ => throw new ArgumentException(
                "O procedimento informado não possui um modelo de ficha.",
                nameof(tipoProcedimento))
        };
    }

    public ModeloFichaSelecionado ObterModeloDaFicha(Ficha ficha)
    {
        ArgumentNullException.ThrowIfNull(ficha);

        if (ficha.VersaoModelo is null ||
            ficha.VersaoQuestionario is null ||
            ficha.VersaoTermo is null ||
            string.IsNullOrWhiteSpace(ficha.CnpjApresentado))
        {
            return new ModeloFichaSelecionado(
                VersaoModelo: 0,
                VersaoQuestionario: 2,
                VersaoTermo: TermoConsentimentoAtual.Versao,
                CnpjApresentado: "Não registrado",
                ConteudoTermo: TermoConsentimentoAtual.Conteudo);
        }

        var modelo = CriarModelo(
            ficha.TipoProcedimento,
            ficha.VersaoTermo.Value,
            ficha.CnpjApresentado);

        return modelo with
        {
            VersaoModelo = ficha.VersaoModelo.Value,
            VersaoQuestionario = ficha.VersaoQuestionario.Value
        };
    }

    private static ModeloFichaSelecionado CriarModelo(
        TipoProcedimento tipoProcedimento,
        int versaoTermo,
        string cnpj)
    {
        var conteudo = (tipoProcedimento, versaoTermo) switch
        {
            (TipoProcedimento.Tatuagem, 1) =>
                TermosProcedimento.ObterTatuagem(cnpj),
            (TipoProcedimento.Piercing, 1) =>
                TermosProcedimento.ObterPiercing(cnpj),
            _ => throw new InvalidOperationException(
                "A versão do modelo desta ficha não está disponível.")
        };

        return new ModeloFichaSelecionado(
            TermosProcedimento.VersaoModeloAtual,
            QuestionarioSaude.VersaoAtual,
            versaoTermo,
            cnpj,
            conteudo);
    }
}
