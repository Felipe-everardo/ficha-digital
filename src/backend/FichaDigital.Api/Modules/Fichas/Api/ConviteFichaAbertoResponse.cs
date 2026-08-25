namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record ConviteFichaAbertoResponse(
    Guid FichaId,
    string Status,
    bool QuestionarioRespondido,
    bool DadosPessoaisPreenchidos,
    string NomeReferencia,
    TermoConsentimentoResponse TermoConsentimento);
