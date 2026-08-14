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
    public async Task Criar_ComDadosValidos_DeveRetornarCreatedEPersistirCliente()
    {
        // Arrange
        using var factory = new FichaDigitalApiFactory();
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        var request = new CriarClienteRequest
        {
            NomeCompleto = "Ana Silva",
            NomeSocial = "Ana",
            Pronomes = "ela/dela",
            DataNascimento = new DateOnly(1995, 6, 15),
            Celular = "(21) 99999-9999",
            Email = "ana@example.com"
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
        Assert.Equal("ela/dela", response.Pronomes);
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

        Assert.Equal("Ana Silva", clientePersistido.NomeCompleto);
        Assert.Equal("Ana", clientePersistido.NomeSocial);
        Assert.Equal("ela/dela", clientePersistido.Pronomes);
        Assert.Equal("(21) 99999-9999", clientePersistido.Celular);
        Assert.Equal("ana@example.com", clientePersistido.Email);
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
            NomeCompleto = "Ana Silva",
            DataNascimento = new DateOnly(1995, 6, 15),
            Celular = "(21) 99999-9999"
        };
    }
}
