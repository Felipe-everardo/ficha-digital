using FichaDigital.Api.Modules.Clientes.Domain;

namespace FichaDigital.Api.Modules.Clientes.Application;

public interface IClienteRepository
{
    Task AdicionarAsync(
        Cliente cliente,
        CancellationToken cancellationToken);
}
