using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Api;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.IntegrationTests.Modules.Fichas.Api;

public sealed class EmitirConviteFichaTests
{
    [Fact]
    public async Task Emitir_ComClienteExistente_DeveRetornarCreated()
    {
        using var factory = new FichaDigitalApiFactory();
        var clienteId = await CriarClienteAsync(factory);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var httpResponse = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
            client,
            $"/api/clientes/{clienteId}/fichas/convites",
            new EmitirConviteFichaRequest
            {
                ProfissionalResponsavelNome = "Lia Tatuadora",
                TipoProcedimento = TipoProcedimento.Tatuagem
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);

        var response = await httpResponse.Content
            .ReadFromJsonAsync<ConviteFichaCriadoResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.FichaId);
        Assert.NotEqual(Guid.Empty, response.ConviteId);
        Assert.StartsWith(
            "/fichas/preencher/",
            response.LinkPreenchimento);
        Assert.True(response.ExpiraEmUtc > DateTimeOffset.UtcNow);
        Assert.Equal(
            $"/api/fichas/{response.FichaId}/convites/{response.ConviteId}",
            httpResponse.Headers.Location?.OriginalString);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var ficha = await dbContext.Fichas
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == response.FichaId,
                TestContext.Current.CancellationToken);
        var profissional = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(profissional.Id, ficha.ProfissionalResponsavelId);
        Assert.Equal("Lia Tatuadora", ficha.ProfissionalResponsavelNome);
        Assert.Equal(TipoProcedimento.Tatuagem, ficha.TipoProcedimento);
        Assert.Equal(1, ficha.VersaoModelo);
        Assert.Equal(QuestionarioSaude.VersaoAtual, ficha.VersaoQuestionario);
        Assert.Equal(1, ficha.VersaoTermo);
        Assert.Equal("00.000.000/0000-00", ficha.CnpjApresentado);
    }

    [Fact]
    public async Task Emitir_ComClienteInexistente_DeveRetornarNotFound()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var httpResponse = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
            client,
            $"/api/clientes/{Guid.NewGuid()}/fichas/convites",
            new EmitirConviteFichaRequest
            {
                ProfissionalResponsavelNome = "Bia Piercer",
                TipoProcedimento = TipoProcedimento.Piercing
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);

        var problemDetails = await httpResponse.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal(
            StatusCodes.Status404NotFound,
            problemDetails.Status);
        Assert.Equal("Cliente não encontrado.", problemDetails.Title);
    }

    [Fact]
    public async Task Emitir_SemProcedimentoValido_DeveRetornarBadRequest()
    {
        using var factory = new FichaDigitalApiFactory();
        var clienteId = await CriarClienteAsync(factory);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
                client,
                $"/api/clientes/{clienteId}/fichas/convites",
                new EmitirConviteFichaRequest
                {
                    ProfissionalResponsavelNome = "Lia Tatuadora",
                    TipoProcedimento = TipoProcedimento.NaoInformado
                },
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Emitir_SemAutenticacao_DeveRetornarUnauthorized()
    {
        using var factory = new FichaDigitalApiFactory();
        var clienteId = await CriarClienteAsync(factory);
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        using var response = await client.PostAsync(
            $"/api/clientes/{clienteId}/fichas/convites",
            content: null,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Emitir_SemAntiforgeryToken_DeveRetornarBadRequest()
    {
        using var factory = new FichaDigitalApiFactory();
        var clienteId = await CriarClienteAsync(factory);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await client.PostAsync(
            $"/api/clientes/{clienteId}/fichas/convites",
            content: null,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<Guid> CriarClienteAsync(
        FichaDigitalApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();

        var cliente = new Cliente(DadosPessoaisTeste.Criar(
            celular: "(21) 99999-9999"));

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        return cliente.Id;
    }
}
