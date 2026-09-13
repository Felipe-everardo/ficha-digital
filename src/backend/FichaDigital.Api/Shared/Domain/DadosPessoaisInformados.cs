namespace FichaDigital.Api.Shared.Domain;

/// <summary>
/// Dados pessoais validados que podem ser copiados tanto para o cadastro
/// reutilizável do cliente quanto para o retrato histórico de uma ficha.
/// </summary>
public sealed class DadosPessoaisInformados
{
    private static readonly HashSet<string> UfsValidas =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO",
        "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI",
        "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    ];

    public DadosPessoaisInformados(
        string nomeCompleto,
        string? nomeSocial,
        string? pronomes,
        string estadoCivil,
        DateOnly dataNascimento,
        string cpf,
        string celular,
        string? telefoneAdicional,
        string? email,
        string? instagram,
        string? contatoEmergenciaNome,
        string? contatoEmergenciaCelular,
        string cep,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string estado)
    {
        NomeCompleto = NormalizarObrigatorio(
            nomeCompleto,
            "O nome completo é obrigatório.",
            nameof(nomeCompleto));
        NomeSocial = NormalizarOpcional(nomeSocial);
        Pronomes = NormalizarOpcional(pronomes);
        EstadoCivil = NormalizarObrigatorio(
            estadoCivil,
            "O estado civil é obrigatório.",
            nameof(estadoCivil));
        DataNascimento = dataNascimento;
        Cpf = NormalizarCpf(cpf);
        Celular = NormalizarObrigatorio(
            celular,
            "O celular é obrigatório.",
            nameof(celular));
        TelefoneAdicional = NormalizarOpcional(telefoneAdicional);
        Email = NormalizarOpcional(email);
        Instagram = NormalizarOpcional(instagram);

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

        ContatoEmergenciaNome = NormalizarOpcional(contatoEmergenciaNome);
        ContatoEmergenciaCelular =
            NormalizarOpcional(contatoEmergenciaCelular);
        Cep = NormalizarCep(cep);
        Logradouro = NormalizarObrigatorio(
            logradouro,
            "O logradouro é obrigatório.",
            nameof(logradouro));
        Numero = NormalizarObrigatorio(
            numero,
            "O número do endereço é obrigatório.",
            nameof(numero));
        Complemento = NormalizarOpcional(complemento);
        Bairro = NormalizarObrigatorio(
            bairro,
            "O bairro é obrigatório.",
            nameof(bairro));
        Cidade = NormalizarObrigatorio(
            cidade,
            "A cidade é obrigatória.",
            nameof(cidade));
        Estado = NormalizarEstado(estado);
    }

    public string NomeCompleto { get; }

    public string? NomeSocial { get; }

    public string? Pronomes { get; }

    public string EstadoCivil { get; }

    public DateOnly DataNascimento { get; }

    public string Cpf { get; }

    public string Celular { get; }

    public string? TelefoneAdicional { get; }

    public string? Email { get; }

    public string? Instagram { get; }

    public string? ContatoEmergenciaNome { get; }

    public string? ContatoEmergenciaCelular { get; }

    public string Cep { get; }

    public string Logradouro { get; }

    public string Numero { get; }

    public string? Complemento { get; }

    public string Bairro { get; }

    public string Cidade { get; }

    public string Estado { get; }

    private static string NormalizarCpf(string valor)
    {
        var cpf = SomenteDigitos(valor);

        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
        {
            throw new ArgumentException("O CPF informado é inválido.", "cpf");
        }

        var primeiroDigito = CalcularDigitoCpf(cpf.AsSpan(0, 9), 10);
        var segundoDigito = CalcularDigitoCpf(cpf.AsSpan(0, 10), 11);

        if (cpf[9] - '0' != primeiroDigito ||
            cpf[10] - '0' != segundoDigito)
        {
            throw new ArgumentException("O CPF informado é inválido.", "cpf");
        }

        return cpf;
    }

    private static int CalcularDigitoCpf(
        ReadOnlySpan<char> digitos,
        int pesoInicial)
    {
        var soma = 0;

        for (var indice = 0; indice < digitos.Length; indice++)
        {
            soma += (digitos[indice] - '0') * (pesoInicial - indice);
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static string NormalizarCep(string valor)
    {
        var cep = SomenteDigitos(valor);

        if (cep.Length != 8)
        {
            throw new ArgumentException("O CEP informado é inválido.", "cep");
        }

        return cep;
    }

    private static string NormalizarEstado(string valor)
    {
        var estado = NormalizarObrigatorio(
            valor,
            "O estado é obrigatório.",
            "estado").ToUpperInvariant();

        if (!UfsValidas.Contains(estado))
        {
            throw new ArgumentException(
                "Informe uma unidade federativa válida.",
                "estado");
        }

        return estado;
    }

    private static string SomenteDigitos(string valor)
    {
        return new string((valor ?? string.Empty)
            .Where(char.IsDigit)
            .ToArray());
    }

    private static string NormalizarObrigatorio(
        string valor,
        string mensagem,
        string nomeParametro)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException(mensagem, nomeParametro);
        }

        return valor.Trim();
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
