namespace FichaDigital.Api.Modules.Financeiro.Api;

public sealed record FinanceiroResponse(
    ResumoFinanceiroResponse Resumo,
    IReadOnlyList<DespesaResponse> Despesas,
    int Pagina,
    int TamanhoPagina,
    int TotalDespesas,
    int TotalPaginas);
