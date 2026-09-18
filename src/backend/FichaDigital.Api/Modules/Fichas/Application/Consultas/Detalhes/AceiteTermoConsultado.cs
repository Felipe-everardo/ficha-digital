namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record AceiteTermoConsultado(
    int VersaoTermo,
    string NomeAssinante,
    DateTimeOffset AceitoEmUtc,
    bool ConfirmouLeituraEAutorizacao,
    string? AssinaturaDesenhada,
    string EvidenciaHash,
    bool EvidenciaIntegra);
