using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.UnitTests.Modules.Fichas.Domain;

public sealed class QuestionarioSaudeTests
{
    [Fact]
    public void Criar_ComRespostasValidas_DeveNormalizarEArmazenarOsDados()
    {
        var fichaId = Guid.NewGuid();
        var antesDaCriacao = DateTimeOffset.UtcNow;

        var questionario = new QuestionarioSaude(
            fichaId,
            temDiabetes: true,
            tipoDiabetes: "  Tipo 1  ",
            possuiPressaoAlta: false,
            temAlergia: true,
            descricaoAlergia: "  Látex  ",
            possuiCondicaoCardiaca: true,
            temEpilepsia: false,
            temHemofilia: true,
            usaMarcaPasso: false,
            estaGravidaOuAmamentando: false);

        var depoisDaCriacao = DateTimeOffset.UtcNow;

        Assert.NotEqual(Guid.Empty, questionario.Id);
        Assert.Equal(fichaId, questionario.FichaId);
        Assert.Equal(QuestionarioSaude.VersaoAtual, questionario.Versao);
        Assert.True(questionario.TemDiabetes);
        Assert.Equal("Tipo 1", questionario.TipoDiabetes);
        Assert.False(questionario.PossuiPressaoAlta);
        Assert.True(questionario.TemAlergia);
        Assert.Equal("Látex", questionario.DescricaoAlergia);
        Assert.True(questionario.PossuiCondicaoCardiaca);
        Assert.False(questionario.TemEpilepsia);
        Assert.True(questionario.TemHemofilia);
        Assert.False(questionario.UsaMarcaPasso);
        Assert.False(questionario.EstaGravidaOuAmamentando);
        Assert.InRange(
            questionario.RespondidoEmUtc,
            antesDaCriacao,
            depoisDaCriacao);
    }

    [Fact]
    public void Criar_ComRespostasNegativas_DeveDescartarDetalhesCondicionais()
    {
        var questionario = new QuestionarioSaude(
            Guid.NewGuid(),
            temDiabetes: false,
            tipoDiabetes: "valor anterior do formulário",
            possuiPressaoAlta: false,
            temAlergia: false,
            descricaoAlergia: "valor anterior do formulário",
            possuiCondicaoCardiaca: false,
            temEpilepsia: false,
            temHemofilia: false,
            usaMarcaPasso: false,
            estaGravidaOuAmamentando: false);

        Assert.Null(questionario.TipoDiabetes);
        Assert.Null(questionario.DescricaoAlergia);
    }

    [Fact]
    public void Criar_ComDiabetesSemTipo_DeveLancarArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new QuestionarioSaude(
                Guid.NewGuid(),
                temDiabetes: true,
                tipoDiabetes: "   ",
                possuiPressaoAlta: false,
                temAlergia: false,
                descricaoAlergia: null,
                possuiCondicaoCardiaca: false,
                temEpilepsia: false,
                temHemofilia: false,
                usaMarcaPasso: false,
                estaGravidaOuAmamentando: false));

        Assert.Equal("tipoDiabetes", exception.ParamName);
        Assert.Contains(
            "O tipo de diabetes é obrigatório quando a resposta for sim.",
            exception.Message);
    }

    [Fact]
    public void Criar_ComAlergiaSemDescricao_DeveLancarArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new QuestionarioSaude(
                Guid.NewGuid(),
                temDiabetes: false,
                tipoDiabetes: null,
                possuiPressaoAlta: false,
                temAlergia: true,
                descricaoAlergia: "   ",
                possuiCondicaoCardiaca: false,
                temEpilepsia: false,
                temHemofilia: false,
                usaMarcaPasso: false,
                estaGravidaOuAmamentando: false));

        Assert.Equal("descricaoAlergia", exception.ParamName);
        Assert.Contains(
            "A descrição da alergia é obrigatória quando a resposta for sim.",
            exception.Message);
    }
}
