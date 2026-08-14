using System.Net.Http.Json;
using FichaDigital.Api.Modules.Profissionais.Api;
using FichaDigital.Api.Modules.Profissionais.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Infrastructure;

public static class AutenticacaoProfissionalTestHelper
{
    private const string Senha = "Senha-Segura-123!";

    public static async Task<HttpClient> CriarClienteAutenticadoAsync(
        FichaDigitalApiFactory factory,
        CancellationToken cancellationToken)
    {
        var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                HandleCookies = true
            });
        var email = $"profissional-{Guid.NewGuid():N}@example.com";

        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ProfissionalUsuario>>();
            var profissional = new ProfissionalUsuario(
                "Profissional de Teste",
                email)
            {
                EmailConfirmed = true
            };
            var resultado = await userManager.CreateAsync(
                profissional,
                Senha);

            if (!resultado.Succeeded)
            {
                client.Dispose();

                throw new InvalidOperationException(string.Join(
                    ", ",
                    resultado.Errors.Select(erro => erro.Description)));
            }
        }

        var antiforgeryToken = await ObterAntiforgeryTokenAsync(
            client,
            cancellationToken);
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/autenticacao/entrar")
        {
            Content = JsonContent.Create(new EntrarProfissionalRequest
            {
                Email = email,
                Senha = Senha
            })
        };
        request.Headers.Add("X-CSRF-TOKEN", antiforgeryToken);
        using var response = await client.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return client;
    }

    public static async Task<HttpResponseMessage> PostProtegidoAsync(
        HttpClient client,
        string requestUri,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        var antiforgeryToken = await ObterAntiforgeryTokenAsync(
            client,
            cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content
        };
        request.Headers.Add("X-CSRF-TOKEN", antiforgeryToken);

        return await client.SendAsync(request, cancellationToken);
    }

    public static Task<HttpResponseMessage> PostComoJsonProtegidoAsync<T>(
        HttpClient client,
        string requestUri,
        T value,
        CancellationToken cancellationToken)
    {
        return PostProtegidoAsync(
            client,
            requestUri,
            JsonContent.Create(value),
            cancellationToken);
    }

    private static async Task<string> ObterAntiforgeryTokenAsync(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(
            "/api/autenticacao/antiforgery-token",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var token = await response.Content
            .ReadFromJsonAsync<AntiforgeryTokenResponse>(
                cancellationToken);

        return token!.Token;
    }
}
