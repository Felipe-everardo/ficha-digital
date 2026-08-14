namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record ClienteFichaDetalheResponse(
    Guid Id,
    string NomeCompleto,
    string? NomeSocial,
    string NomeParaExibicao,
    string? Pronomes,
    DateOnly DataNascimento,
    string Celular,
    string? Email);
