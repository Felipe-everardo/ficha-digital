using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Features.Status;
using FichaDigital.IntegrationTests.Infrastructure;

namespace FichaDigital.IntegrationTests.Features.Status;

public sealed class DatabaseStatusEndpointTests
{
    [Fact]
    public async Task GetDatabase_ComBancoDisponivel_DeveRetornarOk()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = factory.CreateClient();

        using var httpResponse = await client.GetAsync(
            "/api/status/database",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

        var response = await httpResponse.Content
            .ReadFromJsonAsync<DatabaseStatusResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal("Connected", response.Database);
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthChecks_ComAplicacaoSaudavel_DeveRetornarOk(
        string endpoint)
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            endpoint,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Api_DeveRetornarCabecalhosDeSegurancaECorrelacao()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/status",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", response.Headers
            .GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers
            .GetValues("X-Frame-Options").Single());
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }
}
