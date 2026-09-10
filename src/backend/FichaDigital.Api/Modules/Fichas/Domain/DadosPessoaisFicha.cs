namespace FichaDigital.Api.Modules.Fichas.Domain;

/// <summary>
/// Retrato dos dados pessoais confirmados para uma ficha específica.
/// O cadastro do cliente pode continuar evoluindo sem alterar o histórico.
/// </summary>
public sealed class DadosPessoaisFicha
{
    private DadosPessoaisFicha()
    {
    }

    public DadosPessoaisFicha(
        Guid fichaId,
        string nomeCompleto,
        string? nomeSocial,
        string? pronomes,
        DateOnly dataNascimento,
        string celular,
        string? email,
        string? instagram,
        string? contatoEmergenciaNome,
        string? contatoEmergenciaCelular,
        DateTimeOffset confirmadosEmUtc)
    {
        if (fichaId == Guid.Empty)
        {
            throw new ArgumentException("A ficha é obrigatória.", nameof(fichaId));
        }

        FichaId = fichaId;
        Atualizar(
            nomeCompleto,
            nomeSocial,
            pronomes,
            dataNascimento,
            celular,
            email,
            instagram,
            contatoEmergenciaNome,
            contatoEmergenciaCelular,
            confirmadosEmUtc);
    }

    public Guid FichaId { get; private set; }

    public string NomeCompleto { get; private set; } = string.Empty;

    public string? NomeSocial { get; private set; }

    public string? Pronomes { get; private set; }

    public DateOnly DataNascimento { get; private set; }

    public string Celular { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public string? Instagram { get; private set; }

    public string? ContatoEmergenciaNome { get; private set; }

    public string? ContatoEmergenciaCelular { get; private set; }

    public DateTimeOffset ConfirmadosEmUtc { get; private set; }

    public string NomeParaExibicao => NomeSocial ?? NomeCompleto;

    public void Atualizar(
        string nomeCompleto,
        string? nomeSocial,
        string? pronomes,
        DateOnly dataNascimento,
        string celular,
        string? email,
        string? instagram,
        string? contatoEmergenciaNome,
        string? contatoEmergenciaCelular,
        DateTimeOffset confirmadosEmUtc)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
        {
            throw new ArgumentException(
                "O nome completo é obrigatório.",
                nameof(nomeCompleto));
        }

        if (string.IsNullOrWhiteSpace(celular))
        {
            throw new ArgumentException(
                "O celular é obrigatório.",
                nameof(celular));
        }

        if (confirmadosEmUtc == default)
        {
            throw new ArgumentException(
                "A data de confirmação é obrigatória.",
                nameof(confirmadosEmUtc));
        }

        var contatoNomeInformado =
            !string.IsNullOrWhiteSpace(contatoEmergenciaNome);
        var contatoCelularInformado =
            !string.IsNullOrWhiteSpace(contatoEmergenciaCelular);

        if (contatoNomeInformado != contatoCelularInformado)
        {
            throw new ArgumentException(
                "Informe o nome e o celular do contato de emergência.");
        }

        NomeCompleto = nomeCompleto.Trim();
        NomeSocial = NormalizarOpcional(nomeSocial);
        Pronomes = NormalizarOpcional(pronomes);
        DataNascimento = dataNascimento;
        Celular = celular.Trim();
        Email = NormalizarOpcional(email);
        Instagram = NormalizarOpcional(instagram);
        ContatoEmergenciaNome = NormalizarOpcional(contatoEmergenciaNome);
        ContatoEmergenciaCelular = NormalizarOpcional(contatoEmergenciaCelular);
        ConfirmadosEmUtc = confirmadosEmUtc;
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
