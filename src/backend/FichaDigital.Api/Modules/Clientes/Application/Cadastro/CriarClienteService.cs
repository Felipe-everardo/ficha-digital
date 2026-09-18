using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;

namespace FichaDigital.Api.Modules.Clientes.Application;

public sealed class CriarClienteService(FichaDigitalDbContext dbContext)
{
    public async Task<ClienteCriado> CriarAsync(
        CriarClienteCommand command,
        CancellationToken cancellationToken)
    {
        var cliente = new Cliente(command.NomeReferencia);

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ClienteCriado(
            cliente.Id,
            cliente.NomeParaExibicao,
            cliente.CriadoEmUtc);
    }
}
