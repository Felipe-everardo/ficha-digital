namespace FichaDigital.Api.Modules.Clientes.Application;

public interface IConsultaClientes
{
    Task<PaginaClientesConsultada> ListarAsync(
        FiltroConsultaClientes filtro,
        CancellationToken cancellationToken);

    Task<DetalheClienteConsultado?> ObterDetalheAsync(
        Guid clienteId,
        CancellationToken cancellationToken);
}
