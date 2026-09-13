using System.Net;
using FichaDigital.IntegrationTests.Infrastructure;

namespace FichaDigital.IntegrationTests.Features.Status;

public sealed class HealthChecksTests
{
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthCheck_ComAplicacaoSaudavel_DeveRetornarOk(
        string rota)
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            rota,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken));
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        Assert.Equal("nosniff", response.Headers
            .GetValues("X-Content-Type-Options")
            .Single());
        Assert.Equal("DENY", response.Headers
            .GetValues("X-Frame-Options")
            .Single());
    }
}
