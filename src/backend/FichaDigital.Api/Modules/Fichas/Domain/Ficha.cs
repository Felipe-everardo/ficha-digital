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

    public Guid Id { get; private set; }

    public Guid ClienteId { get; private set; }

    public Guid? ProfissionalResponsavelId { get; private set; }

    public string ProfissionalResponsavelNome { get; private set; } =
        string.Empty;

    public TipoProcedimento TipoProcedimento { get; private set; }

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

    public void Concluir()
    {
        if (Status != StatusFicha.EmPreenchimento)
        {
            throw new InvalidOperationException(
                "Somente uma ficha em preenchimento pode ser concluída.");
        }

        Status = StatusFicha.Concluida;
    }
}
