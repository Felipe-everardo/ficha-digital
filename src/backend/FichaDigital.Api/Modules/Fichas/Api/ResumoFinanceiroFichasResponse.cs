namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record ResumoFinanceiroFichasResponse(
    decimal TotalRecebido,
    int AtendimentosRegistrados,
    int FichasSemRegistro);
