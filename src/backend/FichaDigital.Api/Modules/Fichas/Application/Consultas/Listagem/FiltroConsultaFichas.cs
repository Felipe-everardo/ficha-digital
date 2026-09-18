using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record FiltroConsultaFichas(
    string? Busca,
    TipoProcedimento? TipoProcedimento,
    StatusFicha? Status,
    DateOnly? CriadaDe,
    DateOnly? CriadaAte,
    DateOnly? ConcluidaDe,
    DateOnly? ConcluidaAte,
    int Pagina,
    int TamanhoPagina);
