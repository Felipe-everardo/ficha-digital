namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record QuestionarioSaudeRespondidoResponse(
    Guid QuestionarioId,
    Guid FichaId,
    int Versao,
    DateTimeOffset RespondidoEmUtc);
