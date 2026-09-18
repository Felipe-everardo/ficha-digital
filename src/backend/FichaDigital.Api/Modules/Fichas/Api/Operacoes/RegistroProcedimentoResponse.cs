namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record RegistroProcedimentoResponse(
    string TipoProcedimento,
    string ProfissionalNome,
    string? ArteEfetivamenteTatuada,
    string? MaterialUtilizado,
    string? LocalTatuagem,
    string? JoiaUtilizada,
    string? AgulhaUtilizada,
    string? LocalPerfuracao,
    string? Observacoes,
    decimal ValorTotal,
    decimal ValorSinal,
    decimal ValorRestante,
    string FormaPagamento,
    string NomeProfissionalAssinante,
    string AssinaturaDesenhada,
    DateTimeOffset RegistradoEmUtc,
    string EvidenciaHash,
    bool EvidenciaIntegra);
