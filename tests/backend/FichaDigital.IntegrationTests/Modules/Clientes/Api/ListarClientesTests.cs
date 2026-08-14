using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Api;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Modules.Clientes.Api;

public sealed class ListarClientesTests
{
    [Fact]
    public async Task Listar_SemAutenticacao_DeveRetornarUnauthorized()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        using var response = await client.GetAsync(
            "/api/clientes",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Listar_ComPaginacaoValida_DeveRetornarResumoOrdenado()
    {
        using var factory = new FichaDigitalApiFactory();
        await CriarClientesAsync(factory);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await client.GetAsync(
            "/api/clientes?pagina=1&tamanhoPagina=2",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var resultado = await response.Content
            .ReadFromJsonAsync<ClientesPaginadosResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.Pagina);
        Assert.Equal(2, resultado.TamanhoPagina);
        Assert.Equal(3, resultado.TotalItens);
        Assert.Equal(2, resultado.TotalPaginas);
        Assert.Collection(
            resultado.Itens,
            cliente =>
            {
                Assert.Equal("Ana Silva", cliente.NomeParaExibicao);
                Assert.Equal("21911111111", cliente.Celular);
            },
            cliente =>
            {
                Assert.Equal("Bia", cliente.NomeParaExibicao);
                Assert.Equal("Beatriz Souza", cliente.NomeCompleto);
            });
    }

    [Fact]
    public async Task Listar_ComTamanhoAcimaDoLimite_DeveRetornarBadRequest()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await client.GetAsync(
            "/api/clientes?tamanhoPagina=51",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task CriarClientesAsync(
        FichaDigitalApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();

        dbContext.Clientes.AddRange(
            new Cliente(
                "Carlos Lima",
                null,
                "ele/dele",
                new DateOnly(1988, 4, 12),
                "21933333333",
                null),
            new Cliente(
                "Ana Silva",
                null,
                "ela/dela",
                new DateOnly(1995, 6, 15),
                "21911111111",
                "ana@example.com"),
            new Cliente(
                "Beatriz Souza",
                "Bia",
                null,
                new DateOnly(1992, 10, 3),
                "21922222222",
                "bia@example.com"));

        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);
    }
}
