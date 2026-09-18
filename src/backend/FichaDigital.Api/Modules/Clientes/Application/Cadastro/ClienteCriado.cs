namespace FichaDigital.Api.Modules.Clientes.Application;

public sealed record ClienteCriado(
    Guid Id,
    string NomeParaExibicao,
    DateTimeOffset CriadoEmUtc);
