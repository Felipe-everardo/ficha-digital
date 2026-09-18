namespace FichaDigital.Api.Modules.Clientes.Application;

public sealed record ClienteConsultado(
    Guid Id,
    string NomeReferencia,
    string? NomeCompleto,
    string NomeParaExibicao,
    string? Pronomes,
    string? Celular,
    string? Email,
    string? Instagram,
    DateTimeOffset CriadoEmUtc,
    FichaClienteConsultada? UltimaFicha);
