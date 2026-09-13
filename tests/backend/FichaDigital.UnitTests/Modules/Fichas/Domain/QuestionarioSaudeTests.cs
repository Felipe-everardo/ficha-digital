using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.UnitTests.Modules.Fichas.Domain;

public sealed class QuestionarioSaudeTests
{
    [Fact]
    public void Criar_ComRespostasValidas_DeveNormalizarEArmazenarOsDados()
    {
        var questionario = CriarQuestionario(
            temDiabetes: true,
            tipoDiabetes: "  Tipo 1  ",
            teveAnemia: true,
            descricaoAnemia: "  Anemia ferropriva  ",
            teveHepatite: true,
            tipoHepatite: "  Tipo A  ",
            temAlergia: true,
            descricaoAlergia: "  Látex  ",
            possuiDoencaTransmissivel: true,
            descricaoDoencaTransmissivel: "  Informação clínica  ",
            usaMedicacao: true,
            descricaoMedicacao: "  Medicação contínua  ");

        Assert.Equal(QuestionarioSaude.VersaoAtual, questionario.Versao);
        Assert.Equal("Tipo 1", questionario.TipoDiabetes);
        Assert.Equal("Anemia ferropriva", questionario.DescricaoAnemia);
        Assert.Equal("Tipo A", questionario.TipoHepatite);
        Assert.Equal("Látex", questionario.DescricaoAlergia);
        Assert.Equal(
            "Informação clínica",
            questionario.DescricaoDoencaTransmissivel);
        Assert.Equal("Medicação contínua", questionario.DescricaoMedicacao);
    }

    [Fact]
    public void Criar_ComRespostasNegativas_DeveDescartarDetalhesCondicionais()
    {
        var questionario = CriarQuestionario(
            tipoDiabetes: "valor anterior",
            descricaoAnemia: "valor anterior",
            tipoHepatite: "valor anterior",
            descricaoAlergia: "valor anterior",
            descricaoDoencaTransmissivel: "valor anterior",
            descricaoMedicacao: "valor anterior");

        Assert.Null(questionario.TipoDiabetes);
        Assert.Null(questionario.DescricaoAnemia);
        Assert.Null(questionario.TipoHepatite);
        Assert.Null(questionario.DescricaoAlergia);
        Assert.Null(questionario.DescricaoDoencaTransmissivel);
        Assert.Null(questionario.DescricaoMedicacao);
    }

    [Fact]
    public void Criar_ComDiabetesSemTipo_DeveRejeitar()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CriarQuestionario(temDiabetes: true, tipoDiabetes: "   "));

        Assert.Equal("tipoDiabetes", exception.ParamName);
    }

    [Fact]
    public void Criar_ComAnemiaSemDetalhes_DeveRejeitar()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CriarQuestionario(teveAnemia: true, descricaoAnemia: null));

        Assert.Equal("descricaoAnemia", exception.ParamName);
    }

    [Fact]
    public void Criar_ComHepatiteSemTipo_DeveRejeitar()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CriarQuestionario(teveHepatite: true, tipoHepatite: null));

        Assert.Equal("tipoHepatite", exception.ParamName);
    }

    [Fact]
    public void Criar_ComAlergiaSemDescricao_DeveRejeitar()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CriarQuestionario(temAlergia: true, descricaoAlergia: "   "));

        Assert.Equal("descricaoAlergia", exception.ParamName);
    }

    [Fact]
    public void Criar_ComDoencaTransmissivelSemDescricao_DeveRejeitar()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CriarQuestionario(
                possuiDoencaTransmissivel: true,
                descricaoDoencaTransmissivel: null));

        Assert.Equal("descricaoDoencaTransmissivel", exception.ParamName);
    }

    [Fact]
    public void Criar_ComMedicacaoSemDescricao_DeveRejeitar()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CriarQuestionario(usaMedicacao: true, descricaoMedicacao: null));

        Assert.Equal("descricaoMedicacao", exception.ParamName);
    }

    private static QuestionarioSaude CriarQuestionario(
        bool temDiabetes = false,
        string? tipoDiabetes = null,
        bool teveAnemia = false,
        string? descricaoAnemia = null,
        bool teveHepatite = false,
        string? tipoHepatite = null,
        bool temAlergia = false,
        string? descricaoAlergia = null,
        bool possuiDoencaTransmissivel = false,
        string? descricaoDoencaTransmissivel = null,
        bool usaMedicacao = false,
        string? descricaoMedicacao = null)
    {
        return new QuestionarioSaude(
            Guid.NewGuid(),
            temDiabetes,
            tipoDiabetes,
            teveAnemia,
            descricaoAnemia,
            teveHepatite,
            tipoHepatite,
            possuiPressaoAlta: false,
            temAlergia,
            descricaoAlergia,
            possuiCondicaoCardiaca: false,
            temEpilepsia: false,
            temHemofilia: false,
            possuiDoencaTransmissivel,
            descricaoDoencaTransmissivel,
            usaMarcaPasso: false,
            fuma: false,
            consumiuBebidaAlcoolicaUltimas24Horas: false,
            usaMedicacao,
            descricaoMedicacao,
            estaGravidaOuAmamentando: false);
    }
}
