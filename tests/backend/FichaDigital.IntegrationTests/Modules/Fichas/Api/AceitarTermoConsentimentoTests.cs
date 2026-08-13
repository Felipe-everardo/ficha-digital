using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Api;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Modules.Fichas.Api;

public sealed class AceitarTermoConsentimentoTests
{
    [Fact]
    public async Task Aceitar_AposResponderQuestionario_DevePersistirAceiteEConcluirFicha()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var convite = await PrepararConviteAbertoAsync(factory, client);
        await ResponderQuestionarioAsync(client, convite.Token);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/termo-consentimento/aceitar",
            CriarRequestValido(convite),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);

        var response = await httpResponse.Content
            .ReadFromJsonAsync<TermoConsentimentoAceitoResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.AceiteId);
        Assert.Equal(convite.FichaId, response.FichaId);
        Assert.Equal(convite.Termo.Versao, response.VersaoTermo);
        Assert.Equal("Concluida", response.StatusFicha);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var ficha = await dbContext.Fichas
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == convite.FichaId,
                TestContext.Current.CancellationToken);
        var aceite = await dbContext.AceitesTermoConsentimento
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == response.AceiteId,
                TestContext.Current.CancellationToken);

        Assert.Equal(StatusFicha.Concluida, ficha.Status);
        Assert.Equal(convite.Termo.Conteudo, aceite.ConteudoTermo);
        Assert.Equal(convite.Termo.ConteudoHash, aceite.ConteudoHash);
        Assert.Equal("Ana Silva", aceite.NomeAssinante);
    }

    [Fact]
    public async Task Aceitar_SemQuestionario_DeveRetornarConflictENaoConcluirFicha()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var convite = await PrepararConviteAbertoAsync(factory, client);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/termo-consentimento/aceitar",
            CriarRequestValido(convite),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);

        var problemDetails = await httpResponse.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal("Questionário pendente.", problemDetails.Title);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var ficha = await dbContext.Fichas
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == convite.FichaId,
                TestContext.Current.CancellationToken);

        Assert.Equal(StatusFicha.EmPreenchimento, ficha.Status);
        Assert.Empty(await dbContext.AceitesTermoConsentimento.ToListAsync(
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Aceitar_ComHashDesatualizado_DeveRetornarConflict()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var convite = await PrepararConviteAbertoAsync(factory, client);
        await ResponderQuestionarioAsync(client, convite.Token);
        var request = new AceitarTermoConsentimentoRequest
        {
            Token = convite.Token,
            VersaoTermo = convite.Termo.Versao,
            ConteudoHash = new string('0', 64),
            NomeAssinante = "Ana Silva",
            AceitouTermo = true
        };

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/termo-consentimento/aceitar",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);

        var problemDetails = await httpResponse.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal("Termo atualizado.", problemDetails.Title);
    }

    private static AceitarTermoConsentimentoRequest CriarRequestValido(
        ConviteAberto convite)
    {
        return new AceitarTermoConsentimentoRequest
        {
            Token = convite.Token,
            VersaoTermo = convite.Termo.Versao,
            ConteudoHash = convite.Termo.ConteudoHash,
            NomeAssinante = "  Ana Silva  ",
            AceitouTermo = true
        };
    }

    private static async Task<ConviteAberto> PrepararConviteAbertoAsync(
        FichaDigitalApiFactory factory,
        HttpClient client)
    {
        var clienteId = await CriarClienteAsync(factory);
        using var emitirResponse = await client.PostAsync(
            $"/api/clientes/{clienteId}/fichas/convites",
            content: null,
            TestContext.Current.CancellationToken);
        emitirResponse.EnsureSuccessStatusCode();

        var conviteCriado = (await emitirResponse.Content
            .ReadFromJsonAsync<ConviteFichaCriadoResponse>(
                TestContext.Current.CancellationToken))!;
        var token = conviteCriado.LinkPreenchimento.Split('/').Last();

        using var abrirResponse = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = token
            },
            TestContext.Current.CancellationToken);
        abrirResponse.EnsureSuccessStatusCode();

        var conviteAberto = (await abrirResponse.Content
            .ReadFromJsonAsync<ConviteFichaAbertoResponse>(
                TestContext.Current.CancellationToken))!;

        return new ConviteAberto(
            token,
            conviteAberto.FichaId,
            conviteAberto.TermoConsentimento);
    }

    private static async Task ResponderQuestionarioAsync(
        HttpClient client,
        string token)
    {
        using var response = await client.PostAsJsonAsync(
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

        response.EnsureSuccessStatusCode();
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

    private sealed record ConviteAberto(
        string Token,
        Guid FichaId,
        TermoConsentimentoResponse Termo);
}
