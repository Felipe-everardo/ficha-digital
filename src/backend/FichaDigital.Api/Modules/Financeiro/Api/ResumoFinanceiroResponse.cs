namespace FichaDigital.Api.Modules.Financeiro.Api;

public sealed record ResumoFinanceiroResponse(
    decimal TotalRecebido,
    decimal TotalSaidas,
    decimal Saldo,
    int AtendimentosPagos);
