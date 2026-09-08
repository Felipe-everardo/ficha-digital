namespace FichaDigital.Api.Modules.Clientes.Domain;

public sealed class Cliente
{
    private Cliente()
    {
    }

    public Cliente(string nomeReferencia)
    {
        if (string.IsNullOrWhiteSpace(nomeReferencia))
        {
            throw new ArgumentException(
                "O nome de referência é obrigatório.",
                nameof(nomeReferencia));
        }

        Id = Guid.NewGuid();
        NomeReferencia = nomeReferencia.Trim();
        CriadoEmUtc = DateTimeOffset.UtcNow;
    }

    public Cliente(
        string nomeCompleto,
        string? nomeSocial,
        string? pronomes,
        DateOnly dataNascimento,
        string celular,
        string? email)
        : this(nomeCompleto)
    {
        PreencherDadosPessoais(
            nomeCompleto,
            nomeSocial,
            pronomes,
            dataNascimento,
            celular,
            email,
            instagram: null,
            contatoEmergenciaNome: null,
            contatoEmergenciaCelular: null,
            preenchidosEmUtc: DateTimeOffset.UtcNow);
    }

    public Guid Id { get; private set; }

    public string NomeReferencia { get; private set; } = string.Empty;

    public string? NomeCompleto { get; private set; }

    public string? NomeSocial { get; private set; }

    public string NomeParaExibicao =>
        NomeSocial ?? NomeCompleto ?? NomeReferencia;

    public string? Pronomes { get; private set; }

    public DateOnly? DataNascimento { get; private set; }

    public string? Celular { get; private set; }

    public string? Email { get; private set; }

    public string? Instagram { get; private set; }

    public string? ContatoEmergenciaNome { get; private set; }

    public string? ContatoEmergenciaCelular { get; private set; }

    public DateTimeOffset? DadosPessoaisPreenchidosEmUtc { get; private set; }

    public bool DadosPessoaisPreenchidos =>
        DadosPessoaisPreenchidosEmUtc is not null;

    public DateTimeOffset CriadoEmUtc { get; private set; }

    public void PreencherDadosPessoais(
        string nomeCompleto,
        string? nomeSocial,
        string? pronomes,
        DateOnly dataNascimento,
        string celular,
        string? email,
        string? instagram,
        string? contatoEmergenciaNome,
        string? contatoEmergenciaCelular,
        DateTimeOffset preenchidosEmUtc)
    {
        if (DadosPessoaisPreenchidos)
        {
            throw new InvalidOperationException(
                "Os dados pessoais do cliente já foram preenchidos.");
        }

        DefinirDadosPessoais(
            nomeCompleto,
            nomeSocial,
            pronomes,
            dataNascimento,
            celular,
            email,
            instagram,
            contatoEmergenciaNome,
            contatoEmergenciaCelular,
            preenchidosEmUtc);
    }

    public void AtualizarDadosPessoais(
        string nomeCompleto,
        string? nomeSocial,
        string? pronomes,
        DateOnly dataNascimento,
        string celular,
        string? email,
        string? instagram,
        string? contatoEmergenciaNome,
        string? contatoEmergenciaCelular,
        DateTimeOffset atualizadosEmUtc)
    {
        if (!DadosPessoaisPreenchidos)
        {
            throw new InvalidOperationException(
                "Os dados pessoais do cliente ainda não foram preenchidos.");
        }

        DefinirDadosPessoais(
            nomeCompleto,
            nomeSocial,
            pronomes,
            dataNascimento,
            celular,
            email,
            instagram,
            contatoEmergenciaNome,
            contatoEmergenciaCelular,
            atualizadosEmUtc);
    }

    private void DefinirDadosPessoais(
        string nomeCompleto,
        string? nomeSocial,
        string? pronomes,
        DateOnly dataNascimento,
        string celular,
        string? email,
        string? instagram,
        string? contatoEmergenciaNome,
        string? contatoEmergenciaCelular,
        DateTimeOffset preenchidosEmUtc)
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

        if (dataNascimento > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException(
                "A data de nascimento não pode estar no futuro.",
                nameof(dataNascimento));
        }

        var contatoNomeInformado =
            !string.IsNullOrWhiteSpace(contatoEmergenciaNome);
        var contatoCelularInformado =
            !string.IsNullOrWhiteSpace(contatoEmergenciaCelular);

        if (contatoNomeInformado != contatoCelularInformado)
        {
            throw new ArgumentException(
                "Informe o nome e o celular do contato de emergência.",
                contatoNomeInformado
                    ? nameof(contatoEmergenciaCelular)
                    : nameof(contatoEmergenciaNome));
        }

        NomeCompleto = nomeCompleto.Trim();
        NomeSocial = NormalizarOpcional(nomeSocial);
        Pronomes = NormalizarOpcional(pronomes);
        DataNascimento = dataNascimento;
        Celular = celular.Trim();
        Email = NormalizarOpcional(email);
        Instagram = NormalizarOpcional(instagram);
        ContatoEmergenciaNome = NormalizarOpcional(contatoEmergenciaNome);
        ContatoEmergenciaCelular =
            NormalizarOpcional(contatoEmergenciaCelular);
        DadosPessoaisPreenchidosEmUtc = preenchidosEmUtc;
    }

    public void AtualizarContato(string celular, string? email)
    {
        Celular = !string.IsNullOrWhiteSpace(celular)
            ? celular.Trim()
            : throw new ArgumentException(
                "O celular é obrigatório.",
                nameof(celular));
        Email = NormalizarOpcional(email);
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
