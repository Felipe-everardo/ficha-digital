using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Api;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Modules.Fichas.Api;

public sealed class ListarFichasTests
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
            "/api/fichas",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Listar_ComSessaoValida_DeveRetornarResumoSemDadosClinicos()
    {
        var agora = DateTimeOffset.UtcNow;
        using var factory = new FichaDigitalApiFactory(
            new FixedTimeProvider(agora.AddHours(3)));
        var fichas = await CriarFichasAsync(factory, agora);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await client.GetAsync(
            "/api/fichas?pagina=1&tamanhoPagina=10",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);
        var resultado = await response.Content
            .ReadFromJsonAsync<FichasPaginadasResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.TotalItens);
        Assert.Equal(1, resultado.TotalPaginas);

        var fichaExpirada = Assert.Single(
            resultado.Itens,
            ficha => ficha.Id == fichas.FichaExpiradaId);
        Assert.Equal("Ana", fichaExpirada.ClienteNome);
        Assert.Equal("ConviteEnviado", fichaExpirada.Status);
        Assert.True(fichaExpirada.ConviteExpirado);

        var fichaConcluida = Assert.Single(
            resultado.Itens,
            ficha => ficha.Id == fichas.FichaConcluidaId);
        Assert.Equal("Bruno Lima", fichaConcluida.ClienteNome);
        Assert.Equal("Concluida", fichaConcluida.Status);
        Assert.False(fichaConcluida.ConviteExpirado);

        Assert.DoesNotContain("temDiabetes", corpo);
        Assert.DoesNotContain("descricaoAlergia", corpo);
        Assert.DoesNotContain("nomeAssinante", corpo);
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
            "/api/fichas?tamanhoPagina=51",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<FichasCriadas> CriarFichasAsync(
        FichaDigitalApiFactory factory,
        DateTimeOffset agora)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var ana = new Cliente(
            "Ana Silva",
            "Ana",
            "ela/dela",
            new DateOnly(1995, 6, 15),
            "21911111111",
            "ana@example.com");
        var bruno = new Cliente(
            "Bruno Lima",
            null,
            "ele/dele",
            new DateOnly(1990, 3, 20),
            "21922222222",
            "bruno@example.com");
        var fichaExpirada = new Ficha(ana.Id);
        fichaExpirada.EnviarConvite();
        var conviteExpirado = new ConviteFicha(
            fichaExpirada.Id,
            new string('a', 64),
            agora.AddHours(1));
        var fichaConcluida = new Ficha(bruno.Id);
        fichaConcluida.EnviarConvite();
        fichaConcluida.IniciarPreenchimento();
        fichaConcluida.Concluir();
        var conviteConcluido = new ConviteFicha(
            fichaConcluida.Id,
            new string('b', 64),
            agora.AddHours(5));

        dbContext.Clientes.AddRange(ana, bruno);
        dbContext.Fichas.AddRange(fichaExpirada, fichaConcluida);
        dbContext.ConvitesFicha.AddRange(
            conviteExpirado,
            conviteConcluido);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        return new FichasCriadas(
            fichaExpirada.Id,
            fichaConcluida.Id);
    }

    private sealed record FichasCriadas(
        Guid FichaExpiradaId,
        Guid FichaConcluidaId);
}
