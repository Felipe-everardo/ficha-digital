namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record FichaDetalheResponse(
    Guid Id,
    string Status,
    DateTimeOffset CriadaEmUtc,
    DateTimeOffset? ConviteExpiraEmUtc,
    bool ConviteExpirado,
    ClienteFichaDetalheResponse Cliente,
    QuestionarioSaudeDetalheResponse? QuestionarioSaude,
    AceiteTermoResumoResponse? AceiteTermo);
