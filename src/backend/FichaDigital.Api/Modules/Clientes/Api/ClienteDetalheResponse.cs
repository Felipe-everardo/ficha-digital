namespace FichaDigital.Api.Modules.Clientes.Api;

public sealed record ClienteDetalheResponse(
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
    DateTimeOffset? DadosPessoaisPreenchidosEmUtc,
    DateTimeOffset CriadoEmUtc,
    IReadOnlyList<FichaClienteResumoResponse> Fichas);
