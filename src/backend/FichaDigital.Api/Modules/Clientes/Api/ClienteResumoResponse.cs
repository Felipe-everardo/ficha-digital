namespace FichaDigital.Api.Modules.Clientes.Api;

public sealed record ClienteResumoResponse(
    Guid Id,
    string NomeReferencia,
    string? NomeCompleto,
    string NomeParaExibicao,
    string? Pronomes,
    string? Celular,
    string? Email,
    string? Instagram,
    DateTimeOffset CriadoEmUtc,
    FichaClienteResumoResponse? UltimaFicha);
