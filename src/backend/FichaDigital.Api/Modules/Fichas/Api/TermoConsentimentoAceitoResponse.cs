namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record TermoConsentimentoAceitoResponse(
    Guid AceiteId,
    Guid FichaId,
    int VersaoTermo,
    DateTimeOffset AceitoEmUtc,
    string StatusFicha);
