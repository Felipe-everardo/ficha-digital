using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Api;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Modules.Fichas.Api;

public sealed class EmitirConviteFichaTests
{
    [Fact]
    public async Task Emitir_ComClienteExistente_DeveRetornarCreated()
    {
        using var factory = new FichaDigitalApiFactory();
        var clienteId = await CriarClienteAsync(factory);
        using var client = CriarHttpClient(factory);

        using var httpResponse = await client.PostAsync(
            $"/api/clientes/{clienteId}/fichas/convites",
            content: null,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);

        var response = await httpResponse.Content
            .ReadFromJsonAsync<ConviteFichaCriadoResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.FichaId);
        Assert.NotEqual(Guid.Empty, response.ConviteId);
        Assert.StartsWith(
            "/fichas/preencher/",
            response.LinkPreenchimento);
        Assert.True(response.ExpiraEmUtc > DateTimeOffset.UtcNow);
        Assert.Equal(
            $"/api/fichas/{response.FichaId}/convites/{response.ConviteId}",
            httpResponse.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Emitir_ComClienteInexistente_DeveRetornarNotFound()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);

        using var httpResponse = await client.PostAsync(
            $"/api/clientes/{Guid.NewGuid()}/fichas/convites",
            content: null,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);

        var problemDetails = await httpResponse.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal(
            StatusCodes.Status404NotFound,
            problemDetails.Status);
        Assert.Equal("Cliente não encontrado.", problemDetails.Title);
    }

    private static HttpClient CriarHttpClient(
        FichaDigitalApiFactory factory)
    {
        return factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
    }

    private static async Task<Guid> CriarClienteAsync(
        FichaDigitalApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();

        var cliente = new Cliente(
            "Ana Silva",
            "Ana",
            "ela/dela",
            new DateOnly(1995, 6, 15),
            "(21) 99999-9999",
            "ana@example.com");

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        return cliente.Id;
    }
}
