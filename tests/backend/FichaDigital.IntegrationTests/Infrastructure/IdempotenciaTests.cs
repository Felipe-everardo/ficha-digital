using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Infrastructure;

public sealed class IdempotenciaTests
{
    [Fact]
    public async Task PostRepetido_ComMesmaChave_DeveRetornarMesmaRespostaSemDuplicar()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);
        var chave = Guid.NewGuid().ToString();
        var request = new CriarClienteRequest
        {
            NomeReferencia = "Cliente idempotente"
        };

        using var primeira = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
                client,
                "/api/clientes",
                request,
                TestContext.Current.CancellationToken,
                chave);
        using var segunda = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
                client,
                "/api/clientes",
                request,
                TestContext.Current.CancellationToken,
                chave);

        Assert.Equal(HttpStatusCode.Created, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.Created, segunda.StatusCode);
        Assert.True(segunda.Headers.TryGetValues(
            "Idempotency-Replayed",
            out var replay));
        Assert.Equal("true", replay.Single());

        var primeiraResposta = await primeira.Content
            .ReadFromJsonAsync<ClienteCriadoResponse>(
                TestContext.Current.CancellationToken);
        var segundaResposta = await segunda.Content
            .ReadFromJsonAsync<ClienteCriadoResponse>(
                TestContext.Current.CancellationToken);

        Assert.Equal(primeiraResposta, segundaResposta);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        Assert.Equal(1, await dbContext.Clientes.CountAsync(
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task PostRepetido_ComDadosDiferentes_DeveRetornarConflict()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);
        var chave = Guid.NewGuid().ToString();

        using var primeira = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
                client,
                "/api/clientes",
                new CriarClienteRequest { NomeReferencia = "Ana" },
                TestContext.Current.CancellationToken,
                chave);
        using var segunda = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
                client,
                "/api/clientes",
                new CriarClienteRequest { NomeReferencia = "Bia" },
                TestContext.Current.CancellationToken,
                chave);

        Assert.Equal(HttpStatusCode.Created, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
    }
}
