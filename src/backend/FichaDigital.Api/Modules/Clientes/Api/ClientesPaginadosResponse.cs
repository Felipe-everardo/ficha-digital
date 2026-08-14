namespace FichaDigital.Api.Modules.Clientes.Api;

public sealed record ClientesPaginadosResponse(
    IReadOnlyList<ClienteResumoResponse> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas);
