using FichaDigital.Api.Modules.Clientes.Application;
using FichaDigital.Api.Modules.Clientes.Domain;

namespace FichaDigital.UnitTests.Modules.Clientes.Application;

public sealed class CriarClienteServiceTests
{
    [Fact]
    public async Task Criar_DevePersistirClienteERetornarDadosCriados()
    {
        var repository = new ClienteRepositorySpy();
        var service = new CriarClienteService(repository);
        using var cancellationTokenSource = new CancellationTokenSource();

        var resultado = await service.CriarAsync(
            new CriarClienteCommand("  Cliente de teste  "),
            cancellationTokenSource.Token);

        var clientePersistido = Assert.IsType<Cliente>(
            repository.ClienteAdicionado);
        Assert.Equal("Cliente de teste", clientePersistido.NomeReferencia);
        Assert.Equal(clientePersistido.Id, resultado.Id);
        Assert.Equal(clientePersistido.NomeParaExibicao,
            resultado.NomeParaExibicao);
        Assert.Equal(clientePersistido.CriadoEmUtc, resultado.CriadoEmUtc);
        Assert.Equal(
            cancellationTokenSource.Token,
            repository.CancellationTokenRecebido);
    }

    private sealed class ClienteRepositorySpy : IClienteRepository
    {
        public Cliente? ClienteAdicionado { get; private set; }

        public CancellationToken CancellationTokenRecebido { get; private set; }

        public Task AdicionarAsync(
            Cliente cliente,
            CancellationToken cancellationToken)
        {
            ClienteAdicionado = cliente;
            CancellationTokenRecebido = cancellationToken;
            return Task.CompletedTask;
        }
    }
}
