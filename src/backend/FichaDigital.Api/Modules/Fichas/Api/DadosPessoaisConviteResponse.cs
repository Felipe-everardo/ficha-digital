namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record DadosPessoaisConviteResponse(
    string? NomeCompleto,
    string? NomeSocial,
    string? Pronomes,
    DateOnly? DataNascimento,
    string? Celular,
    string? Email,
    string? Instagram,
    string? ContatoEmergenciaNome,
    string? ContatoEmergenciaCelular);
