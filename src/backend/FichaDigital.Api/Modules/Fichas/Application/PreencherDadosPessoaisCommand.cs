namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record PreencherDadosPessoaisCommand(
    string TokenOriginal,
    string NomeCompleto,
    string? NomeSocial,
    string? Pronomes,
    DateOnly DataNascimento,
    string Celular,
    string? Email,
    string? Instagram,
    string? ContatoEmergenciaNome,
    string? ContatoEmergenciaCelular);
