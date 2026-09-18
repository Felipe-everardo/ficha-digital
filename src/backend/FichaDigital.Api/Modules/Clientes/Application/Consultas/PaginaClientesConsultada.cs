namespace FichaDigital.Api.Modules.Clientes.Application;

public sealed record PaginaClientesConsultada(
    IReadOnlyList<ClienteConsultado> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas);
