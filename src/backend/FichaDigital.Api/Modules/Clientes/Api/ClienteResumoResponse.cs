namespace FichaDigital.Api.Modules.Clientes.Api;

public sealed record ClienteResumoResponse(
    Guid Id,
    string NomeCompleto,
    string NomeParaExibicao,
    string? Pronomes,
    string Celular,
    string? Email,
    DateTimeOffset CriadoEmUtc);
