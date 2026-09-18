namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record PaginaFichasConsultada(
    IReadOnlyList<FichaConsultada> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas);
