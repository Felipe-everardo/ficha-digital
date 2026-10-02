using System.Net;
using FichaDigital.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FichaDigital.IntegrationTests.Infrastructure;

namespace FichaDigital.IntegrationTests.Features.Status;

public sealed class HealthChecksTests
{
    [Fact]
    public async Task Ready_ComColunaAusente_DeveFalharSemAfetarLiveness()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FichaDigitalDbContext>();
        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE Fichas DROP COLUMN VersaoConcorrencia",
            TestContext.Current.CancellationToken);

        using var ready = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);
        using var live = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, ready.StatusCode);
        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
    }

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
