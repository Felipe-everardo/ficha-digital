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
}
