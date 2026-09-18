namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record FichaConsultada(
    Guid Id,
    Guid ClienteId,
    string ClienteNome,
    Guid? ProfissionalResponsavelId,
    string ProfissionalResponsavelNome,
    string TipoProcedimento,
    string Status,
    DateTimeOffset CriadaEmUtc,
    DateTimeOffset? ConcluidaEmUtc,
    DateTimeOffset? ConviteExpiraEmUtc,
    bool ConviteExpirado);
