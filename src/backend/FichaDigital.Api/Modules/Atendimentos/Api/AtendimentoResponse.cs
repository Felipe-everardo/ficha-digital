namespace FichaDigital.Api.Modules.Atendimentos.Api;

public sealed record AtendimentoResponse(
    Guid Id,
    Guid FichaId,
    DateOnly DataRealizacao,
    decimal ValorCobrado,
    decimal Desconto,
    decimal ValorFinal,
    string FormaPagamento,
    string SituacaoPagamento,
    DateTimeOffset RegistradoEmUtc,
    DateTimeOffset AtualizadoEmUtc);
