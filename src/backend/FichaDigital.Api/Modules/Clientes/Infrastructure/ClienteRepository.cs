using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Application;
using FichaDigital.Api.Modules.Clientes.Domain;

namespace FichaDigital.Api.Modules.Clientes.Infrastructure;

internal sealed class ClienteRepository(FichaDigitalDbContext dbContext)
    : IClienteRepository
{
    public async Task AdicionarAsync(
        Cliente cliente,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cliente);

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
