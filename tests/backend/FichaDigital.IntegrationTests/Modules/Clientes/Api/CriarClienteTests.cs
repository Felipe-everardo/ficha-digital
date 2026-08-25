using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Api;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Modules.Clientes.Api;

public sealed class CriarClienteTests
{
    [Fact]
    public async Task Criar_ComNomeDeReferenciaValido_DeveRetornarCreatedEPersistirClientePendente()
    {
        // Arrange
        using var factory = new FichaDigitalApiFactory();
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        var request = new CriarClienteRequest
        {
            NomeReferencia = "  Ana  "
        };

        // Act
        using var httpResponse = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
            client,
            "/api/clientes",
            request,
            TestContext.Current.CancellationToken);

        // Assert: resposta HTTP
        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);

        var response = await httpResponse.Content
            .ReadFromJsonAsync<ClienteCriadoResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Ana", response.NomeParaExibicao);
        Assert.Equal(
            $"/api/clientes/{response.Id}",
            httpResponse.Headers.Location?.OriginalString);

        // Assert: persistência
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();

        var clientePersistido = await dbContext.Clientes
            .AsNoTracking()
            .SingleAsync(
                cliente => cliente.Id == response.Id,
                TestContext.Current.CancellationToken);

        Assert.Equal("Ana", clientePersistido.NomeReferencia);
        Assert.Null(clientePersistido.NomeCompleto);
        Assert.Null(clientePersistido.Celular);
        Assert.False(clientePersistido.DadosPessoaisPreenchidos);
    }

    [Fact]
    public async Task Criar_SemAutenticacao_DeveRetornarUnauthorized()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        using var response = await client.PostAsJsonAsync(
            "/api/clientes",
            CriarRequestValido(),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Criar_SemAntiforgeryToken_DeveRetornarBadRequest()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await client.PostAsJsonAsync(
            "/api/clientes",
            CriarRequestValido(),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static CriarClienteRequest CriarRequestValido()
    {
        return new CriarClienteRequest
        {
            NomeReferencia = "Ana"
        };
    }
}
