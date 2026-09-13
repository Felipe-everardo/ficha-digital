namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record AceiteTermoResumoResponse(
    int VersaoTermo,
    string NomeAssinante,
    DateTimeOffset AceitoEmUtc,
    bool ConfirmouLeituraEAutorizacao,
    string? AssinaturaDesenhada,
    string EvidenciaHash,
    bool EvidenciaIntegra);
