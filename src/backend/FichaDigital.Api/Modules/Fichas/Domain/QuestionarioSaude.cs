namespace FichaDigital.Api.Modules.Fichas.Domain;

public sealed class QuestionarioSaude
{
    public const int VersaoAtual = 3;

    private const int TamanhoMaximoTipoDiabetes = 100;
    private const int TamanhoMaximoDetalheSaude = 300;
    private const int TamanhoMaximoTipoHepatite = 100;
    private const int TamanhoMaximoDescricaoAlergia = 300;

    private QuestionarioSaude()
    {
    }

    public QuestionarioSaude(
        Guid fichaId,
        bool temDiabetes,
        string? tipoDiabetes,
        bool teveAnemia,
        string? descricaoAnemia,
        bool teveHepatite,
        string? tipoHepatite,
        bool possuiPressaoAlta,
        bool temAlergia,
        string? descricaoAlergia,
        bool possuiCondicaoCardiaca,
        bool temEpilepsia,
        bool temHemofilia,
        bool possuiDoencaTransmissivel,
        string? descricaoDoencaTransmissivel,
        bool usaMarcaPasso,
        bool fuma,
        bool consumiuBebidaAlcoolicaUltimas24Horas,
        bool usaMedicacao,
        string? descricaoMedicacao,
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
            teveAnemia,
            descricaoAnemia,
            TamanhoMaximoDetalheSaude,
            "Os detalhes sobre a anemia são obrigatórios quando a resposta for sim.",
            "Os detalhes sobre a anemia devem ter no máximo 300 caracteres.",
            nameof(descricaoAnemia));

        ValidarDetalheCondicional(
            teveHepatite,
            tipoHepatite,
            TamanhoMaximoTipoHepatite,
            "O tipo de hepatite é obrigatório quando a resposta for sim.",
            "O tipo de hepatite deve ter no máximo 100 caracteres.",
            nameof(tipoHepatite));

        ValidarDetalheCondicional(
            temAlergia,
            descricaoAlergia,
            TamanhoMaximoDescricaoAlergia,
            "A descrição da alergia é obrigatória quando a resposta for sim.",
            "A descrição da alergia deve ter no máximo 300 caracteres.",
            nameof(descricaoAlergia));

        ValidarDetalheCondicional(
            possuiDoencaTransmissivel,
            descricaoDoencaTransmissivel,
            TamanhoMaximoDetalheSaude,
            "A doença transmissível é obrigatória quando a resposta for sim.",
            "A descrição da doença transmissível deve ter no máximo 300 caracteres.",
            nameof(descricaoDoencaTransmissivel));

        ValidarDetalheCondicional(
            usaMedicacao,
            descricaoMedicacao,
            TamanhoMaximoDetalheSaude,
            "A medicação utilizada é obrigatória quando a resposta for sim.",
            "A descrição da medicação deve ter no máximo 300 caracteres.",
            nameof(descricaoMedicacao));

        Id = Guid.NewGuid();
        FichaId = fichaId;
        Versao = VersaoAtual;
        TemDiabetes = temDiabetes;
        TipoDiabetes = temDiabetes
            ? tipoDiabetes!.Trim()
            : null;
        TeveAnemia = teveAnemia;
        DescricaoAnemia = teveAnemia
            ? descricaoAnemia!.Trim()
            : null;
        TeveHepatite = teveHepatite;
        TipoHepatite = teveHepatite
            ? tipoHepatite!.Trim()
            : null;
        PossuiPressaoAlta = possuiPressaoAlta;
        TemAlergia = temAlergia;
        DescricaoAlergia = temAlergia
            ? descricaoAlergia!.Trim()
            : null;
        PossuiCondicaoCardiaca = possuiCondicaoCardiaca;
        TemEpilepsia = temEpilepsia;
        TemHemofilia = temHemofilia;
        PossuiDoencaTransmissivel = possuiDoencaTransmissivel;
        DescricaoDoencaTransmissivel = possuiDoencaTransmissivel
            ? descricaoDoencaTransmissivel!.Trim()
            : null;
        UsaMarcaPasso = usaMarcaPasso;
        Fuma = fuma;
        ConsumiuBebidaAlcoolicaUltimas24Horas =
            consumiuBebidaAlcoolicaUltimas24Horas;
        UsaMedicacao = usaMedicacao;
        DescricaoMedicacao = usaMedicacao
            ? descricaoMedicacao!.Trim()
            : null;
        EstaGravidaOuAmamentando = estaGravidaOuAmamentando;
        RespondidoEmUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid FichaId { get; private set; }

    public int Versao { get; private set; }

    public bool TemDiabetes { get; private set; }

    public string? TipoDiabetes { get; private set; }

    public bool TeveAnemia { get; private set; }

    public string? DescricaoAnemia { get; private set; }

    public bool TeveHepatite { get; private set; }

    public string? TipoHepatite { get; private set; }

    public bool PossuiPressaoAlta { get; private set; }

    public bool TemAlergia { get; private set; }

    public string? DescricaoAlergia { get; private set; }

    public bool PossuiCondicaoCardiaca { get; private set; }

    public bool TemEpilepsia { get; private set; }

    public bool TemHemofilia { get; private set; }

    public bool PossuiDoencaTransmissivel { get; private set; }

    public string? DescricaoDoencaTransmissivel { get; private set; }

    public bool UsaMarcaPasso { get; private set; }

    public bool Fuma { get; private set; }

    public bool ConsumiuBebidaAlcoolicaUltimas24Horas { get; private set; }

    public bool UsaMedicacao { get; private set; }

    public string? DescricaoMedicacao { get; private set; }

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
