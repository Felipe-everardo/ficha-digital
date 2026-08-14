using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Modules.Profissionais.Api;
using FichaDigital.Api.Modules.Profissionais.Domain;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Modules.Profissionais.Api;

public sealed class AutenticacaoProfissionaisTests
{
    private const string Email = "lia@example.com";
    private const string Senha = "Senha-Segura-123!";

    [Fact]
    public async Task ObterSessao_SemAutenticacao_DeveRetornarUnauthorized()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);

        using var response = await client.GetAsync(
            "/api/autenticacao/sessao",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Entrar_ComCredenciaisValidas_DeveCriarSessaoProtegida()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var profissional = await CriarProfissionalAsync(factory);
        var antiforgeryToken = await ObterAntiforgeryTokenAsync(client);

        using var response = await EntrarAsync(
            client,
            Email,
            Senha,
            antiforgeryToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var sessao = await response.Content
            .ReadFromJsonAsync<SessaoProfissionalResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(sessao);
        Assert.Equal(profissional.Id, sessao.ProfissionalId);
        Assert.Equal("Lia Silva", sessao.NomeCompleto);
        Assert.Equal(Email, sessao.Email);

        var cookies = response.Headers.GetValues("Set-Cookie");
        Assert.Contains(cookies, cookie =>
            cookie.StartsWith(
                "FichaDigital.Profissional=",
                StringComparison.Ordinal) &&
            cookie.Contains("httponly", StringComparison.OrdinalIgnoreCase) &&
            cookie.Contains("secure", StringComparison.OrdinalIgnoreCase) &&
            cookie.Contains("samesite=strict", StringComparison.OrdinalIgnoreCase));

        using var sessaoResponse = await client.GetAsync(
            "/api/autenticacao/sessao",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, sessaoResponse.StatusCode);
    }

    [Fact]
    public async Task Entrar_SemAntiforgeryToken_DeveRetornarBadRequest()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        await CriarProfissionalAsync(factory);

        using var response = await client.PostAsJsonAsync(
            "/api/autenticacao/entrar",
            new EntrarProfissionalRequest
            {
                Email = Email,
                Senha = Senha
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Entrar_ComSenhaInvalida_DeveRetornarMensagemGenerica()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        await CriarProfissionalAsync(factory);
        var antiforgeryToken = await ObterAntiforgeryTokenAsync(client);

        using var response = await EntrarAsync(
            client,
            Email,
            "senha-incorreta",
            antiforgeryToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var problem = await response.Content
            .ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problem);
        Assert.Equal("Não foi possível entrar.", problem.Title);
    }

    [Fact]
    public async Task Sair_ComSessaoValida_DeveEncerrarSessao()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        await CriarProfissionalAsync(factory);
        var tokenLogin = await ObterAntiforgeryTokenAsync(client);

        using var loginResponse = await EntrarAsync(
            client,
            Email,
            Senha,
            tokenLogin);

        loginResponse.EnsureSuccessStatusCode();

        var tokenLogout = await ObterAntiforgeryTokenAsync(client);
        using var logoutRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/autenticacao/sair");
        logoutRequest.Headers.Add("X-CSRF-TOKEN", tokenLogout);

        using var logoutResponse = await client.SendAsync(
            logoutRequest,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        using var sessaoResponse = await client.GetAsync(
            "/api/autenticacao/sessao",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, sessaoResponse.StatusCode);
    }

    [Fact]
    public async Task Entrar_AposCincoFalhas_DeveManterContaBloqueada()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        await CriarProfissionalAsync(factory);
        var antiforgeryToken = await ObterAntiforgeryTokenAsync(client);

        for (var tentativa = 1; tentativa <= 5; tentativa++)
        {
            using var falha = await EntrarAsync(
                client,
                Email,
                "senha-incorreta",
                antiforgeryToken);

            Assert.Equal(HttpStatusCode.Unauthorized, falha.StatusCode);
        }

        using var respostaComSenhaCorreta = await EntrarAsync(
            client,
            Email,
            Senha,
            antiforgeryToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            respostaComSenhaCorreta.StatusCode);
    }

    private static HttpClient CriarHttpClient(
        FichaDigitalApiFactory factory)
    {
        return factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                HandleCookies = true
            });
    }

    private static async Task<ProfissionalUsuario> CriarProfissionalAsync(
        FichaDigitalApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ProfissionalUsuario>>();
        var profissional = new ProfissionalUsuario(
            "Lia Silva",
            Email)
        {
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(
            profissional,
            Senha);

        Assert.True(
            result.Succeeded,
            string.Join(", ", result.Errors.Select(error => error.Description)));

        return profissional;
    }

    private static async Task<string> ObterAntiforgeryTokenAsync(
        HttpClient client)
    {
        using var response = await client.GetAsync(
            "/api/autenticacao/antiforgery-token",
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        var token = await response.Content
            .ReadFromJsonAsync<AntiforgeryTokenResponse>(
                TestContext.Current.CancellationToken);

        return token!.Token;
    }

    private static async Task<HttpResponseMessage> EntrarAsync(
        HttpClient client,
        string email,
        string senha,
        string antiforgeryToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/autenticacao/entrar")
        {
            Content = JsonContent.Create(new EntrarProfissionalRequest
            {
                Email = email,
                Senha = senha
            })
        };
        request.Headers.Add("X-CSRF-TOKEN", antiforgeryToken);

        return await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);
    }
}
