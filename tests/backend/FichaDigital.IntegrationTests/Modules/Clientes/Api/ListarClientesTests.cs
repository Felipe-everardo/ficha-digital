using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Api;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Profissionais.Domain;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
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

    [Fact]
    public async Task Listar_ComBusca_DeveRetornarSomenteCorrespondentes()
    {
        using var factory = new FichaDigitalApiFactory();
        await CriarClientesAsync(factory);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await client.GetAsync(
            "/api/clientes?busca=Bia",
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var resultado = await response.Content
            .ReadFromJsonAsync<ClientesPaginadosResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(resultado);
        var cliente = Assert.Single(resultado.Itens);
        Assert.Equal("Bia", cliente.NomeParaExibicao);
    }

    [Theory]
    [InlineData("A", "Ana Silva")]
    [InlineData("C", "Carlos Lima")]
    public async Task Listar_ComInicialDoNome_NaoDeveEncontrarTextoNoMeioDoEmail(
        string busca,
        string nomeEsperado)
    {
        using var factory = new FichaDigitalApiFactory();
        await CriarClientesAsync(factory);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await client.GetAsync(
            $"/api/clientes?busca={busca}",
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var resultado = await response.Content
            .ReadFromJsonAsync<ClientesPaginadosResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(resultado);
        var cliente = Assert.Single(resultado.Itens);
        Assert.Equal(nomeEsperado, cliente.NomeParaExibicao);
    }

    [Theory]
    [InlineData("ana@example.com")]
    [InlineData("21911111111")]
    public async Task Listar_ComEmailOuTelefone_NaoDeveUsarEssesDadosNaBusca(
        string busca)
    {
        using var factory = new FichaDigitalApiFactory();
        await CriarClientesAsync(factory);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await client.GetAsync(
            $"/api/clientes?busca={Uri.EscapeDataString(busca)}",
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var resultado = await response.Content
            .ReadFromJsonAsync<ClientesPaginadosResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(resultado);
        Assert.Empty(resultado.Itens);
    }

    [Fact]
    public async Task Listar_ComFiltroDeProcedimento_DeveConsiderarSomenteUltimaFicha()
    {
        using var factory = new FichaDigitalApiFactory();
        var clienteId = await CriarClienteComHistoricoAsync(factory);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var responseAtual = await client.GetAsync(
            "/api/clientes?tipoProcedimento=Piercing",
            TestContext.Current.CancellationToken);
        using var responseAntiga = await client.GetAsync(
            "/api/clientes?tipoProcedimento=Tatuagem",
            TestContext.Current.CancellationToken);

        responseAtual.EnsureSuccessStatusCode();
        responseAntiga.EnsureSuccessStatusCode();
        var resultadoAtual = await responseAtual.Content
            .ReadFromJsonAsync<ClientesPaginadosResponse>(
                TestContext.Current.CancellationToken);
        var resultadoAntigo = await responseAntiga.Content
            .ReadFromJsonAsync<ClientesPaginadosResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(resultadoAtual);
        Assert.NotNull(resultadoAntigo);
        var cliente = Assert.Single(resultadoAtual.Itens);
        Assert.Equal(clienteId, cliente.Id);
        Assert.NotNull(cliente.UltimaFicha);
        Assert.Equal("Piercing", cliente.UltimaFicha.TipoProcedimento);
        Assert.Empty(resultadoAntigo.Itens);
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

    private static async Task<Guid> CriarClienteComHistoricoAsync(
        FichaDigitalApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ProfissionalUsuario>>();
        var profissional = new ProfissionalUsuario(
            "Profissional do Histórico",
            $"historico-{Guid.NewGuid():N}@example.com");
        var criacaoProfissional = await userManager.CreateAsync(profissional);
        Assert.True(criacaoProfissional.Succeeded);
        var cliente = new Cliente("Cliente recorrente");
        var fichaAntiga = new Ficha(
            cliente.Id,
            profissional.Id,
            profissional.NomeCompleto,
            TipoProcedimento.Tatuagem);
        await Task.Delay(5, TestContext.Current.CancellationToken);
        var fichaAtual = new Ficha(
            cliente.Id,
            profissional.Id,
            profissional.NomeCompleto,
            TipoProcedimento.Piercing);

        dbContext.Clientes.Add(cliente);
        dbContext.Fichas.AddRange(fichaAntiga, fichaAtual);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        return cliente.Id;
    }
}
