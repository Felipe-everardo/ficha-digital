namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record FichasPaginadasResponse(
    IReadOnlyList<FichaResumoResponse> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas);
