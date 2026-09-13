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

public sealed class ObterDetalheClienteTests
{
    [Fact]
    public async Task ObterDetalhe_SemAutenticacao_DeveRetornarUnauthorized()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        using var response = await client.GetAsync(
            $"/api/clientes/{Guid.NewGuid()}",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ObterDetalhe_DeveRetornarDadosEHistoricoDoCliente()
    {
        using var factory = new FichaDigitalApiFactory();
        var clienteId = await CriarClienteComFichaAsync(factory);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await client.GetAsync(
            $"/api/clientes/{clienteId}",
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.CacheControl?.NoStore);
        var detalhe = await response.Content
            .ReadFromJsonAsync<ClienteDetalheResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(detalhe);
        Assert.Equal("Ana", detalhe.NomeParaExibicao);
        var ficha = Assert.Single(detalhe.Fichas);
        Assert.Equal("Tatuagem", ficha.TipoProcedimento);
        Assert.Equal("Marina Tattoo", ficha.ProfissionalResponsavelNome);
    }

    private static async Task<Guid> CriarClienteComFichaAsync(
        FichaDigitalApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ProfissionalUsuario>>();
        var profissional = new ProfissionalUsuario(
            "Marina Tattoo",
            $"marina-{Guid.NewGuid():N}@example.com");
        var criacao = await userManager.CreateAsync(profissional);
        Assert.True(criacao.Succeeded);

        var cliente = new Cliente(DadosPessoaisTeste.Criar(
            celular: "21911111111"));
        var ficha = new Ficha(
            cliente.Id,
            profissional.Id,
            profissional.NomeCompleto,
            TipoProcedimento.Tatuagem);
        ficha.EnviarConvite();

        dbContext.Clientes.Add(cliente);
        dbContext.Fichas.Add(ficha);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        return cliente.Id;
    }
}
