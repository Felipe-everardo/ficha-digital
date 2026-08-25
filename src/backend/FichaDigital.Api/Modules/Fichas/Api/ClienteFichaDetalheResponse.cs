namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record ClienteFichaDetalheResponse(
    Guid Id,
    string NomeReferencia,
    string? NomeCompleto,
    string? NomeSocial,
    string NomeParaExibicao,
    string? Pronomes,
    DateOnly? DataNascimento,
    string? Celular,
    string? Email,
    string? Instagram,
    string? ContatoEmergenciaNome,
    string? ContatoEmergenciaCelular,
    DateTimeOffset? DadosPessoaisPreenchidosEmUtc);
