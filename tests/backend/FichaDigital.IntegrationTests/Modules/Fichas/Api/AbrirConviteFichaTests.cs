using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Api;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Modules.Fichas.Api;

public sealed class AbrirConviteFichaTests
{
    [Fact]
    public async Task Abrir_ComConviteValido_DeveIniciarPreenchimento()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var conviteEmitido = await EmitirConviteAsync(factory, client);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = ObterToken(conviteEmitido.LinkPreenchimento)
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

        var response = await httpResponse.Content
            .ReadFromJsonAsync<ConviteFichaAbertoResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(conviteEmitido.FichaId, response.FichaId);
        Assert.Equal("EmPreenchimento", response.Status);
        Assert.False(response.QuestionarioRespondido);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var ficha = await dbContext.Fichas
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == response.FichaId,
                TestContext.Current.CancellationToken);

        Assert.Equal(StatusFicha.EmPreenchimento, ficha.Status);
    }

    [Fact]
    public async Task Abrir_DuasVezes_DeveContinuarDisponivel()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var conviteEmitido = await EmitirConviteAsync(factory, client);
        var request = new AbrirConviteFichaRequest
        {
            Token = ObterToken(conviteEmitido.LinkPreenchimento)
        };

        using var primeiraResposta = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            request,
            TestContext.Current.CancellationToken);
        using var segundaResposta = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, primeiraResposta.StatusCode);
        Assert.Equal(HttpStatusCode.OK, segundaResposta.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();

        Assert.Equal(1, await dbContext.Fichas.CountAsync(
            TestContext.Current.CancellationToken));
        Assert.Equal(1, await dbContext.ConvitesFicha.CountAsync(
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Abrir_AposResponderQuestionario_DeveInformarEtapaConcluida()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var conviteEmitido = await EmitirConviteAsync(factory, client);
        var token = ObterToken(conviteEmitido.LinkPreenchimento);

        using var aberturaInicial = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = token
            },
            TestContext.Current.CancellationToken);

        aberturaInicial.EnsureSuccessStatusCode();

        using var respostaQuestionario = await client.PostAsJsonAsync(
            "/api/fichas/questionario-saude",
            new ResponderQuestionarioSaudeRequest
            {
                Token = token,
                TemDiabetes = false,
                PossuiPressaoAlta = false,
                TemAlergia = false,
                PossuiCondicaoCardiaca = false,
                TemEpilepsia = false,
                TemHemofilia = false,
                UsaMarcaPasso = false,
                EstaGravidaOuAmamentando = false
            },
            TestContext.Current.CancellationToken);

        respostaQuestionario.EnsureSuccessStatusCode();

        using var reabertura = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = token
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, reabertura.StatusCode);

        var response = await reabertura.Content
            .ReadFromJsonAsync<ConviteFichaAbertoResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.True(response.QuestionarioRespondido);
        Assert.Equal("EmPreenchimento", response.Status);
    }

    [Fact]
    public async Task Abrir_ComTokenInvalido_DeveRetornarNotFound()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = "token-que-nao-existe"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);

        var problemDetails = await httpResponse.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal("Convite inválido.", problemDetails.Title);
    }

    [Fact]
    public async Task Abrir_ComConviteExpirado_DeveRetornarGone()
    {
        var expiraEmUtc = DateTimeOffset.UtcNow.AddHours(1);
        var timeProvider = new FixedTimeProvider(
            expiraEmUtc.AddMinutes(1));
        using var factory = new FichaDigitalApiFactory(timeProvider);
        using var client = CriarHttpClient(factory);
        var tokenOriginal = await CriarConviteAsync(
            factory,
            expiraEmUtc);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = tokenOriginal
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Gone, httpResponse.StatusCode);

        var problemDetails = await httpResponse.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal("Convite expirado.", problemDetails.Title);
    }

    [Fact]
    public async Task Abrir_AposExcederLimite_DeveRetornarTooManyRequests()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var request = new AbrirConviteFichaRequest
        {
            Token = "token-inexistente"
        };

        for (var tentativa = 1; tentativa <= 10; tentativa++)
        {
            using var respostaPermitida = await client.PostAsJsonAsync(
                "/api/fichas/convites/abrir",
                request,
                TestContext.Current.CancellationToken);

            Assert.Equal(
                HttpStatusCode.NotFound,
                respostaPermitida.StatusCode);
        }

        using var respostaBloqueada = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            respostaBloqueada.StatusCode);

        var problemDetails = await respostaBloqueada.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal("Muitas tentativas.", problemDetails.Title);
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

    private static async Task<ConviteFichaCriadoResponse> EmitirConviteAsync(
        FichaDigitalApiFactory factory,
        HttpClient client)
    {
        var clienteId = await CriarClienteAsync(factory);
        using var response = await client.PostAsync(
            $"/api/clientes/{clienteId}/fichas/convites",
            content: null,
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        return (await response.Content
            .ReadFromJsonAsync<ConviteFichaCriadoResponse>(
                TestContext.Current.CancellationToken))!;
    }

    private static string ObterToken(string linkPreenchimento)
    {
        return linkPreenchimento.Split('/').Last();
    }

    private static async Task<Guid> CriarClienteAsync(
        FichaDigitalApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var cliente = CriarCliente();

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        return cliente.Id;
    }

    private static async Task<string> CriarConviteAsync(
        FichaDigitalApiFactory factory,
        DateTimeOffset expiraEmUtc)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var geradorToken = scope.ServiceProvider
            .GetRequiredService<GeradorTokenConvite>();
        var cliente = CriarCliente();
        var ficha = new Ficha(cliente.Id);
        ficha.EnviarConvite();
        var token = geradorToken.Gerar();
        var convite = new ConviteFicha(
            ficha.Id,
            token.TokenHash,
            expiraEmUtc);

        dbContext.Clientes.Add(cliente);
        dbContext.Fichas.Add(ficha);
        dbContext.ConvitesFicha.Add(convite);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        return token.TokenOriginal;
    }

    private static Cliente CriarCliente()
    {
        return new Cliente(
            "Ana Silva",
            "Ana",
            "ela/dela",
            new DateOnly(1995, 6, 15),
            "(21) 99999-9999",
            "ana@example.com");
    }
}
