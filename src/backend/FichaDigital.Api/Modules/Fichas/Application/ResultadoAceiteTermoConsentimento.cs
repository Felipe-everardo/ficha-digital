namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record ResultadoAceiteTermoConsentimento(
    StatusAceiteTermoConsentimento Resultado,
    Guid? AceiteId = null,
    Guid? FichaId = null,
    int? VersaoTermo = null,
    DateTimeOffset? AceitoEmUtc = null);
