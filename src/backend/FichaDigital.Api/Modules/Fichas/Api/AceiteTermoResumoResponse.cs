namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record AceiteTermoResumoResponse(
    int VersaoTermo,
    string NomeAssinante,
    DateTimeOffset AceitoEmUtc,
    bool ConfirmouMaioridade,
    bool ConfirmouDadosPessoais,
    bool ConfirmouQuestionarioSaude,
    string EvidenciaHash,
    bool EvidenciaIntegra);
