namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record DadosPessoaisConviteResponse(
    string? NomeCompleto,
    string? NomeSocial,
    string? Pronomes,
    string? EstadoCivil,
    DateOnly? DataNascimento,
    string? Cpf,
    string? Celular,
    string? TelefoneAdicional,
    string? Email,
    string? Instagram,
    string? ContatoEmergenciaNome,
    string? ContatoEmergenciaCelular,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado);
