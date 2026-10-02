namespace FichaDigital.Api.Modules.Fichas.Application;

public interface IConsultaListagemFichas
{
    Task<PaginaFichasConsultada> ListarAsync(
        FiltroConsultaFichas filtro,
        CancellationToken cancellationToken);
}
