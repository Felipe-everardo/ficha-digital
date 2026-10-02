namespace FichaDigital.Api.Modules.Fichas.Application;

public interface IConsultaDetalheFicha
{
    Task<DetalheFichaConsultada?> ObterAsync(
        Guid fichaId,
        CancellationToken cancellationToken);
}
