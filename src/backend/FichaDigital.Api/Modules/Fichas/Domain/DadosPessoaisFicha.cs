using FichaDigital.Api.Shared.Domain;

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
        DadosPessoaisInformados dados,
        DateTimeOffset confirmadosEmUtc)
    {
        if (fichaId == Guid.Empty)
        {
            throw new ArgumentException("A ficha é obrigatória.", nameof(fichaId));
        }

        FichaId = fichaId;
        Atualizar(dados, confirmadosEmUtc);
    }

    public Guid FichaId { get; private set; }

    public string NomeCompleto { get; private set; } = string.Empty;

    public string? NomeSocial { get; private set; }

    public string? Pronomes { get; private set; }

    public string? EstadoCivil { get; private set; }

    public DateOnly DataNascimento { get; private set; }

    public string? Cpf { get; private set; }

    public string Celular { get; private set; } = string.Empty;

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

    public DateTimeOffset ConfirmadosEmUtc { get; private set; }

    public string NomeParaExibicao => NomeSocial ?? NomeCompleto;

    public void Atualizar(
        DadosPessoaisInformados dados,
        DateTimeOffset confirmadosEmUtc)
    {
        if (confirmadosEmUtc == default)
        {
            throw new ArgumentException(
                "A data de confirmação é obrigatória.",
                nameof(confirmadosEmUtc));
        }

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
        ConfirmadosEmUtc = confirmadosEmUtc;
    }
}
