using FichaDigital.Api.Shared.Domain;

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

    public Cliente(DadosPessoaisInformados dados)
        : this(dados?.NomeCompleto ?? throw new ArgumentNullException(nameof(dados)))
    {
        PreencherDadosPessoais(dados!, DateTimeOffset.UtcNow);
    }

    public Guid Id { get; private set; }

    public string NomeReferencia { get; private set; } = string.Empty;

    public string? NomeCompleto { get; private set; }

    public string? NomeSocial { get; private set; }

    public string NomeParaExibicao =>
        NomeSocial ?? NomeCompleto ?? NomeReferencia;

    public string? Pronomes { get; private set; }

    public string? EstadoCivil { get; private set; }

    public DateOnly? DataNascimento { get; private set; }

    public string? Cpf { get; private set; }

    public string? Celular { get; private set; }

    public string? TelefoneAdicional { get; private set; }

    public string? Email { get; private set; }

    public string? Instagram { get; private set; }

    public string? ContatoEmergenciaNome { get; private set; }

    public string? ContatoEmergenciaCelular { get; private set; }

    public string? Cep { get; private set; }

    public string? Logradouro { get; private set; }

    public string? Numero { get; private set; }

    public string? Complemento { get; private set; }

    public string? Bairro { get; private set; }

    public string? Cidade { get; private set; }

    public string? Estado { get; private set; }

    public DateTimeOffset? DadosPessoaisPreenchidosEmUtc { get; private set; }

    public bool DadosPessoaisPreenchidos =>
        DadosPessoaisPreenchidosEmUtc is not null;

    public DateTimeOffset CriadoEmUtc { get; private set; }

    public void PreencherDadosPessoais(
        DadosPessoaisInformados dados,
        DateTimeOffset preenchidosEmUtc)
    {
        if (DadosPessoaisPreenchidos)
        {
            throw new InvalidOperationException(
                "Os dados pessoais do cliente já foram preenchidos.");
        }

        DefinirDadosPessoais(dados, preenchidosEmUtc);
    }

    public void AtualizarDadosPessoais(
        DadosPessoaisInformados dados,
        DateTimeOffset atualizadosEmUtc)
    {
        if (!DadosPessoaisPreenchidos)
        {
            throw new InvalidOperationException(
                "Os dados pessoais do cliente ainda não foram preenchidos.");
        }

        DefinirDadosPessoais(dados, atualizadosEmUtc);
    }

    private void DefinirDadosPessoais(
        DadosPessoaisInformados dados,
        DateTimeOffset preenchidosEmUtc)
    {
        ArgumentNullException.ThrowIfNull(dados);

        NomeCompleto = dados.NomeCompleto;
        NomeSocial = dados.NomeSocial;
        Pronomes = dados.Pronomes;
        EstadoCivil = dados.EstadoCivil;
        DataNascimento = dados.DataNascimento;
        Cpf = dados.Cpf;
        Celular = dados.Celular;
        TelefoneAdicional = dados.TelefoneAdicional;
        Email = dados.Email;
        Instagram = dados.Instagram;
        ContatoEmergenciaNome = dados.ContatoEmergenciaNome;
        ContatoEmergenciaCelular = dados.ContatoEmergenciaCelular;
        Cep = dados.Cep;
        Logradouro = dados.Logradouro;
        Numero = dados.Numero;
        Complemento = dados.Complemento;
        Bairro = dados.Bairro;
        Cidade = dados.Cidade;
        Estado = dados.Estado;
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
