namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record DadosPessoaisConvite(
    string? NomeCompleto,
    string? NomeSocial,
    string? Pronomes,
    DateOnly? DataNascimento,
    string? Celular,
    string? Email,
    string? Instagram,
    string? ContatoEmergenciaNome,
    string? ContatoEmergenciaCelular);
