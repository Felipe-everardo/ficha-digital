using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Clientes.Application;

public sealed record FiltroConsultaClientes(
    string? Busca,
    string? Telefone,
    string? Instagram,
    TipoProcedimento? TipoProcedimento,
    DateOnly? UltimaFichaDe,
    DateOnly? UltimaFichaAte,
    int Pagina,
    int TamanhoPagina);
