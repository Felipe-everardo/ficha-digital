namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record ConviteFichaAbertoResponse(
    Guid FichaId,
    string Status,
    bool QuestionarioRespondido,
    TermoConsentimentoResponse TermoConsentimento);
