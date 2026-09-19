using FichaDigital.Api.Modules.Clientes.Domain;

namespace FichaDigital.Api.Modules.Clientes.Application;

public sealed class CriarClienteService(IClienteRepository clienteRepository)
{
    public async Task<ClienteCriado> CriarAsync(
        CriarClienteCommand command,
        CancellationToken cancellationToken)
    {
        var cliente = new Cliente(command.NomeReferencia);

        await clienteRepository.AdicionarAsync(cliente, cancellationToken);

        return new ClienteCriado(
            cliente.Id,
            cliente.NomeParaExibicao,
            cliente.CriadoEmUtc);
    }
}
