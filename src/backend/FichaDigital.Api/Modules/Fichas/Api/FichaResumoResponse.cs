namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record FichaResumoResponse(
    Guid Id,
    Guid ClienteId,
    string ClienteNome,
    string Status,
    DateTimeOffset CriadaEmUtc,
    DateTimeOffset? ConviteExpiraEmUtc,
    bool ConviteExpirado);
