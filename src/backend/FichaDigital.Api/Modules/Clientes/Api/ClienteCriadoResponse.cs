namespace FichaDigital.Api.Modules.Clientes.Api;

public sealed record ClienteCriadoResponse(
    Guid Id,
    string NomeParaExibicao,
    DateTimeOffset CriadoEmUtc);
