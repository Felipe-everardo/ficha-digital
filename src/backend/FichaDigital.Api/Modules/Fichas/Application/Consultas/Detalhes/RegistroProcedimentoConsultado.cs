namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record RegistroProcedimentoConsultado(
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
    string FormaPagamento,
    string NomeProfissionalAssinante,
    string AssinaturaDesenhada,
    DateTimeOffset RegistradoEmUtc,
    string EvidenciaHash,
    bool EvidenciaIntegra);
