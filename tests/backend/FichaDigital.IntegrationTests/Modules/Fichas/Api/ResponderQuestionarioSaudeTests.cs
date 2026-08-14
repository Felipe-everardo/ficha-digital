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

public sealed class ResponderQuestionarioSaudeTests
{
    [Fact]
    public async Task Responder_ComDadosValidos_DeveRetornarCreatedEPersistirQuestionario()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var (token, fichaId) = await PrepararFichaEmPreenchimentoAsync(
            factory,
            client);
        var request = CriarRequestValido(token);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/questionario-saude",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);

        var response = await httpResponse.Content
            .ReadFromJsonAsync<QuestionarioSaudeRespondidoResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.QuestionarioId);
        Assert.Equal(fichaId, response.FichaId);
        Assert.Equal(2, response.Versao);
        Assert.Equal(
            $"/api/fichas/{fichaId}/questionario-saude",
            httpResponse.Headers.Location?.OriginalString);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var questionario = await dbContext.QuestionariosSaude
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == response.QuestionarioId,
                TestContext.Current.CancellationToken);

        Assert.Equal(fichaId, questionario.FichaId);
        Assert.True(questionario.TemDiabetes);
        Assert.Equal("Tipo 1", questionario.TipoDiabetes);
        Assert.False(questionario.PossuiPressaoAlta);
        Assert.True(questionario.TemAlergia);
        Assert.Equal("Látex", questionario.DescricaoAlergia);
        Assert.True(questionario.PossuiCondicaoCardiaca);
        Assert.False(questionario.TemEpilepsia);
        Assert.False(questionario.TemHemofilia);
        Assert.True(questionario.UsaMarcaPasso);
        Assert.False(questionario.EstaGravidaOuAmamentando);
    }

    [Fact]
    public async Task Responder_DuasVezes_DeveRetornarConflictENaoDuplicarQuestionario()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var (token, _) = await PrepararFichaEmPreenchimentoAsync(
            factory,
            client);
        var request = CriarRequestValido(token);

        using var primeiraResposta = await client.PostAsJsonAsync(
            "/api/fichas/questionario-saude",
            request,
            TestContext.Current.CancellationToken);
        using var segundaResposta = await client.PostAsJsonAsync(
            "/api/fichas/questionario-saude",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, primeiraResposta.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, segundaResposta.StatusCode);

        var problemDetails = await segundaResposta.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal("Questionário já respondido.", problemDetails.Title);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();

        Assert.Equal(1, await dbContext.QuestionariosSaude.CountAsync(
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Responder_ComTokenInvalido_DeveRetornarNotFound()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var request = CriarRequestValido("token-inexistente");

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/questionario-saude",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);

        var problemDetails = await httpResponse.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal("Convite inválido.", problemDetails.Title);
    }

    [Fact]
    public async Task Responder_AntesDeAbrirConvite_DeveRetornarConflict()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var token = await CriarConviteDiretamenteAsync(
            factory,
            DateTimeOffset.UtcNow.AddHours(1),
            iniciarPreenchimento: false);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/questionario-saude",
            CriarRequestValido(token),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);

        var problemDetails = await httpResponse.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal("Ficha indisponível.", problemDetails.Title);
    }

    [Fact]
    public async Task Responder_ComConviteExpirado_DeveRetornarGone()
    {
        var expiraEmUtc = DateTimeOffset.UtcNow.AddHours(1);
        using var factory = new FichaDigitalApiFactory(
            new FixedTimeProvider(expiraEmUtc.AddMinutes(1)));
        using var client = CriarHttpClient(factory);
        var token = await CriarConviteDiretamenteAsync(
            factory,
            expiraEmUtc,
            iniciarPreenchimento: true);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/questionario-saude",
            CriarRequestValido(token),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Gone, httpResponse.StatusCode);

        var problemDetails = await httpResponse.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal("Convite expirado.", problemDetails.Title);
    }

    [Fact]
    public async Task Responder_ComDiabetesSemTipo_DeveRetornarBadRequest()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var request = new ResponderQuestionarioSaudeRequest
        {
            Token = "token-nao-chegara-ao-servico",
            TemDiabetes = true,
            TipoDiabetes = null,
            PossuiPressaoAlta = false,
            TemAlergia = false,
            DescricaoAlergia = null,
            PossuiCondicaoCardiaca = false,
            TemEpilepsia = false,
            TemHemofilia = false,
            UsaMarcaPasso = false,
            EstaGravidaOuAmamentando = false
        };

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/questionario-saude",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);

        var validationProblem = await httpResponse.Content
            .ReadFromJsonAsync<ValidationProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(validationProblem);
        Assert.Contains(
            nameof(ResponderQuestionarioSaudeRequest.TipoDiabetes),
            validationProblem.Errors.Keys);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();

        Assert.Empty(await dbContext.QuestionariosSaude.ToListAsync(
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Responder_SemRespostaSobreMarcaPasso_DeveRetornarBadRequest()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var request = new ResponderQuestionarioSaudeRequest
        {
            Token = "token-nao-chegara-ao-servico",
            TemDiabetes = false,
            TipoDiabetes = null,
            PossuiPressaoAlta = false,
            TemAlergia = false,
            DescricaoAlergia = null,
            PossuiCondicaoCardiaca = false,
            TemEpilepsia = false,
            TemHemofilia = false,
            EstaGravidaOuAmamentando = false
        };

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/questionario-saude",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);

        var validationProblem = await httpResponse.Content
            .ReadFromJsonAsync<ValidationProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(validationProblem);
        Assert.Contains(
            nameof(ResponderQuestionarioSaudeRequest.UsaMarcaPasso),
            validationProblem.Errors.Keys);
    }

    private static ResponderQuestionarioSaudeRequest CriarRequestValido(
        string token)
    {
        return new ResponderQuestionarioSaudeRequest
        {
            Token = token,
            TemDiabetes = true,
            TipoDiabetes = "  Tipo 1  ",
            PossuiPressaoAlta = false,
            TemAlergia = true,
            DescricaoAlergia = "  Látex  ",
            PossuiCondicaoCardiaca = true,
            TemEpilepsia = false,
            TemHemofilia = false,
            UsaMarcaPasso = true,
            EstaGravidaOuAmamentando = false
        };
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

    private static async Task<(string Token, Guid FichaId)>
        PrepararFichaEmPreenchimentoAsync(
            FichaDigitalApiFactory factory,
            HttpClient client)
        {
        var clienteId = await CriarClienteAsync(factory);
        using var clientProfissional = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);
        using var emitirResponse = await AutenticacaoProfissionalTestHelper
            .PostProtegidoAsync(
            clientProfissional,
            $"/api/clientes/{clienteId}/fichas/convites",
            content: null,
            TestContext.Current.CancellationToken);
        emitirResponse.EnsureSuccessStatusCode();

        var convite = (await emitirResponse.Content
            .ReadFromJsonAsync<ConviteFichaCriadoResponse>(
                TestContext.Current.CancellationToken))!;
        var token = convite.LinkPreenchimento.Split('/').Last();

        using var abrirResponse = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = token
            },
            TestContext.Current.CancellationToken);
        abrirResponse.EnsureSuccessStatusCode();

        return (token, convite.FichaId);
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

    private static async Task<string> CriarConviteDiretamenteAsync(
        FichaDigitalApiFactory factory,
        DateTimeOffset expiraEmUtc,
        bool iniciarPreenchimento)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var geradorToken = scope.ServiceProvider
            .GetRequiredService<GeradorTokenConvite>();
        var cliente = new Cliente(
            "Ana Silva",
            "Ana",
            "ela/dela",
            new DateOnly(1995, 6, 15),
            "(21) 99999-9999",
            "ana@example.com");
        var ficha = new Ficha(cliente.Id);
        ficha.EnviarConvite();

        if (iniciarPreenchimento)
        {
            ficha.IniciarPreenchimento();
        }

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
}
