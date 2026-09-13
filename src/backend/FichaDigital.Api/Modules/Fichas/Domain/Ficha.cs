namespace FichaDigital.Api.Modules.Fichas.Domain;

public sealed class Ficha
{
    private Ficha()
    {
    }

    public Ficha(Guid clienteId)
        : this(
            clienteId,
            null,
            "Profissional não informado",
            TipoProcedimento.NaoInformado)
    {
    }

    public Ficha(
        Guid clienteId,
        Guid? profissionalResponsavelId,
        string profissionalResponsavelNome,
        TipoProcedimento tipoProcedimento)
    {
        if (clienteId == Guid.Empty)
        {
            throw new ArgumentException(
                "O cliente é obrigatório.",
                nameof(clienteId));
        }

        if (profissionalResponsavelId == Guid.Empty)
        {
            throw new ArgumentException(
                "O profissional responsável é inválido.",
                nameof(profissionalResponsavelId));
        }

        if (string.IsNullOrWhiteSpace(profissionalResponsavelNome))
        {
            throw new ArgumentException(
                "O nome do profissional responsável é obrigatório.",
                nameof(profissionalResponsavelNome));
        }

        if (!Enum.IsDefined(tipoProcedimento) ||
            (profissionalResponsavelId is not null &&
             tipoProcedimento == TipoProcedimento.NaoInformado) ||
            (profissionalResponsavelId is null &&
             tipoProcedimento != TipoProcedimento.NaoInformado))
        {
            throw new ArgumentException(
                "O procedimento informado é inválido.",
                nameof(tipoProcedimento));
        }

        Id = Guid.NewGuid();
        ClienteId = clienteId;
        ProfissionalResponsavelId = profissionalResponsavelId;
        ProfissionalResponsavelNome =
            profissionalResponsavelNome.Trim();
        TipoProcedimento = tipoProcedimento;
        Status = StatusFicha.Rascunho;
        CriadaEmUtc = DateTimeOffset.UtcNow;
    }

    public Ficha(
        Guid clienteId,
        Guid profissionalResponsavelId,
        string profissionalResponsavelNome,
        TipoProcedimento tipoProcedimento,
        int versaoModelo,
        int versaoQuestionario,
        int versaoTermo,
        string cnpjApresentado)
        : this(
            clienteId,
            profissionalResponsavelId,
            profissionalResponsavelNome,
            tipoProcedimento)
    {
        if (versaoModelo <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(versaoModelo),
                "A versão do modelo deve ser maior que zero.");
        }

        if (versaoQuestionario <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(versaoQuestionario),
                "A versão do questionário deve ser maior que zero.");
        }

        if (versaoTermo <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(versaoTermo),
                "A versão do termo deve ser maior que zero.");
        }

        if (string.IsNullOrWhiteSpace(cnpjApresentado))
        {
            throw new ArgumentException(
                "O CNPJ apresentado é obrigatório.",
                nameof(cnpjApresentado));
        }

        if (cnpjApresentado.Trim().Length > 18)
        {
            throw new ArgumentException(
                "O CNPJ apresentado deve ter no máximo 18 caracteres.",
                nameof(cnpjApresentado));
        }

        VersaoModelo = versaoModelo;
        VersaoQuestionario = versaoQuestionario;
        VersaoTermo = versaoTermo;
        CnpjApresentado = cnpjApresentado.Trim();
    }

    public Guid Id { get; private set; }

    public Guid ClienteId { get; private set; }

    public Guid? ProfissionalResponsavelId { get; private set; }

    public string ProfissionalResponsavelNome { get; private set; } =
        string.Empty;

    public TipoProcedimento TipoProcedimento { get; private set; }

    public int? VersaoModelo { get; private set; }

    public int? VersaoQuestionario { get; private set; }

    public int? VersaoTermo { get; private set; }

    public string? CnpjApresentado { get; private set; }

    public StatusFicha Status { get; private set; }

    public DateTimeOffset CriadaEmUtc { get; private set; }

    public void EnviarConvite()
    {
        if (Status != StatusFicha.Rascunho)
        {
            throw new InvalidOperationException(
                "Somente uma ficha em rascunho pode ter o convite enviado.");
        }

        Status = StatusFicha.ConviteEnviado;
    }

    public void IniciarPreenchimento()
    {
        if (Status != StatusFicha.ConviteEnviado)
        {
            throw new InvalidOperationException(
                "Somente uma ficha com convite enviado pode iniciar o preenchimento.");
        }

        Status = StatusFicha.EmPreenchimento;
    }

    public void ConcluirAnamnese()
    {
        if (Status != StatusFicha.EmPreenchimento)
        {
            throw new InvalidOperationException(
                "Somente uma ficha em preenchimento pode concluir a anamnese.");
        }

        Status = StatusFicha.AnamnesePreenchida;
    }

    public void AutorizarProcedimento()
    {
        if (Status is not (
                StatusFicha.AnamnesePreenchida or
                StatusFicha.AguardandoConsentimento))
        {
            throw new InvalidOperationException(
                "Somente uma ficha com anamnese preenchida pode autorizar o procedimento.");
        }

        Status = StatusFicha.AutorizadaParaProcedimento;
    }

    public void ConfirmarRevisaoProfissional()
    {
        if (Status != StatusFicha.AutorizadaParaProcedimento)
        {
            throw new InvalidOperationException(
                "Somente uma ficha autorizada pode ser revisada pelo profissional.");
        }

        Status = StatusFicha.RevisadaPeloProfissional;
    }

    public void ConcluirProcedimento()
    {
        if (Status != StatusFicha.RevisadaPeloProfissional)
        {
            throw new InvalidOperationException(
                "Somente uma ficha revisada pelo profissional pode ser concluída.");
        }

        Status = StatusFicha.Concluida;
    }

    // Compatibilidade temporária com a ficha atual. Deve ser removido quando
    // o fluxo de consentimento prévio e registro pós-procedimento estiver ativo.
    public void ConcluirFluxoLegado()
    {
        if (Status != StatusFicha.EmPreenchimento)
        {
            throw new InvalidOperationException(
                "Somente uma ficha em preenchimento pode concluir o fluxo legado.");
        }

        Status = StatusFicha.Concluida;
    }
}
