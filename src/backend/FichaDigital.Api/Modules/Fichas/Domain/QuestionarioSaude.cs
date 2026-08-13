namespace FichaDigital.Api.Modules.Fichas.Domain;

public sealed class QuestionarioSaude
{
    public const int VersaoAtual = 2;

    private const int TamanhoMaximoTipoDiabetes = 100;
    private const int TamanhoMaximoDescricaoAlergia = 300;

    private QuestionarioSaude()
    {
    }

    public QuestionarioSaude(
        Guid fichaId,
        bool temDiabetes,
        string? tipoDiabetes,
        bool possuiPressaoAlta,
        bool temAlergia,
        string? descricaoAlergia,
        bool possuiCondicaoCardiaca,
        bool temEpilepsia,
        bool temHemofilia,
        bool usaMarcaPasso,
        bool estaGravidaOuAmamentando)
    {
        if (fichaId == Guid.Empty)
        {
            throw new ArgumentException(
                "A ficha é obrigatória.",
                nameof(fichaId));
        }

        ValidarDetalheCondicional(
            temDiabetes,
            tipoDiabetes,
            TamanhoMaximoTipoDiabetes,
            "O tipo de diabetes é obrigatório quando a resposta for sim.",
            "O tipo de diabetes deve ter no máximo 100 caracteres.",
            nameof(tipoDiabetes));

        ValidarDetalheCondicional(
            temAlergia,
            descricaoAlergia,
            TamanhoMaximoDescricaoAlergia,
            "A descrição da alergia é obrigatória quando a resposta for sim.",
            "A descrição da alergia deve ter no máximo 300 caracteres.",
            nameof(descricaoAlergia));

        Id = Guid.NewGuid();
        FichaId = fichaId;
        Versao = VersaoAtual;
        TemDiabetes = temDiabetes;
        TipoDiabetes = temDiabetes
            ? tipoDiabetes!.Trim()
            : null;
        PossuiPressaoAlta = possuiPressaoAlta;
        TemAlergia = temAlergia;
        DescricaoAlergia = temAlergia
            ? descricaoAlergia!.Trim()
            : null;
        PossuiCondicaoCardiaca = possuiCondicaoCardiaca;
        TemEpilepsia = temEpilepsia;
        TemHemofilia = temHemofilia;
        UsaMarcaPasso = usaMarcaPasso;
        EstaGravidaOuAmamentando = estaGravidaOuAmamentando;
        RespondidoEmUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid FichaId { get; private set; }

    public int Versao { get; private set; }

    public bool TemDiabetes { get; private set; }

    public string? TipoDiabetes { get; private set; }

    public bool PossuiPressaoAlta { get; private set; }

    public bool TemAlergia { get; private set; }

    public string? DescricaoAlergia { get; private set; }

    public bool PossuiCondicaoCardiaca { get; private set; }

    public bool TemEpilepsia { get; private set; }

    public bool TemHemofilia { get; private set; }

    public bool UsaMarcaPasso { get; private set; }

    public bool EstaGravidaOuAmamentando { get; private set; }

    public DateTimeOffset RespondidoEmUtc { get; private set; }

    private static void ValidarDetalheCondicional(
        bool resposta,
        string? detalhe,
        int tamanhoMaximo,
        string mensagemObrigatorio,
        string mensagemTamanhoMaximo,
        string nomeParametro)
    {
        if (!resposta)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(detalhe))
        {
            throw new ArgumentException(
                mensagemObrigatorio,
                nomeParametro);
        }

        if (detalhe.Trim().Length > tamanhoMaximo)
        {
            throw new ArgumentException(
                mensagemTamanhoMaximo,
                nomeParametro);
        }
    }
}
