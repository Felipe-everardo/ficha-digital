using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Atendimentos.Api;
using FichaDigital.Api.Modules.Atendimentos.Domain;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Modules.Atendimentos.Api;

public sealed class RegistrarAtendimentoTests
{
    [Fact]
    public async Task Registrar_SemAutenticacao_DeveRetornarUnauthorized()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        using var response = await client.PutAsJsonAsync(
            $"/api/fichas/{Guid.NewGuid()}/atendimento",
            CriarRequestValido(),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Registrar_ComFichaConcluida_DeveCriarEAtualizarAtendimento()
    {
        var agora = DateTimeOffset.UtcNow;
        using var factory = new FichaDigitalApiFactory(
            new FixedTimeProvider(agora));
        var fichaId = await CriarFichaAsync(factory, concluida: true);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);
        var request = CriarRequestValido();

        using var responseCriacao = await AutenticacaoProfissionalTestHelper
            .PutComoJsonProtegidoAsync(
                client,
                $"/api/fichas/{fichaId}/atendimento",
                request,
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, responseCriacao.StatusCode);
        var criado = await responseCriacao.Content
            .ReadFromJsonAsync<AtendimentoResponse>(
                TestContext.Current.CancellationToken);
        Assert.NotNull(criado);
        Assert.Equal(200m, criado.ValorCobrado);
        Assert.Equal(20m, criado.Desconto);
        Assert.Equal(180m, criado.ValorFinal);
        Assert.Equal("Pix", criado.FormaPagamento);
        Assert.Equal("Pago", criado.SituacaoPagamento);

        var atualizacao = new RegistrarAtendimentoRequest
        {
            DataRealizacao = request.DataRealizacao,
            ValorCobrado = 250m,
            Desconto = 0m,
            FormaPagamento = FormaPagamento.CartaoCredito
        };
        using var responseAtualizacao = await AutenticacaoProfissionalTestHelper
            .PutComoJsonProtegidoAsync(
                client,
                $"/api/fichas/{fichaId}/atendimento",
                atualizacao,
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, responseAtualizacao.StatusCode);
        var atualizado = await responseAtualizacao.Content
            .ReadFromJsonAsync<AtendimentoResponse>(
                TestContext.Current.CancellationToken);
        Assert.NotNull(atualizado);
        Assert.Equal(criado.Id, atualizado.Id);
        Assert.Equal(250m, atualizado.ValorFinal);
        Assert.Equal("CartaoCredito", atualizado.FormaPagamento);
        Assert.Equal("Pago", atualizado.SituacaoPagamento);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var persistido = await dbContext.Atendimentos
            .AsNoTracking()
            .SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(fichaId, persistido.FichaId);
        Assert.Equal(250m, persistido.ValorFinal);
    }

    [Fact]
    public async Task Registrar_ComFichaPendente_DeveRetornarConflict()
    {
        using var factory = new FichaDigitalApiFactory();
        var fichaId = await CriarFichaAsync(factory, concluida: false);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await AutenticacaoProfissionalTestHelper
            .PutComoJsonProtegidoAsync(
                client,
                $"/api/fichas/{fichaId}/atendimento",
                CriarRequestValido(),
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Registrar_ComDescontoMaiorQueValor_DeveRetornarBadRequest()
    {
        using var factory = new FichaDigitalApiFactory();
        var fichaId = await CriarFichaAsync(factory, concluida: true);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);
        var request = new RegistrarAtendimentoRequest
        {
            DataRealizacao = DateOnly.FromDateTime(DateTime.UtcNow),
            ValorCobrado = 100m,
            Desconto = 120m,
            FormaPagamento = FormaPagamento.Dinheiro
        };

        using var response = await AutenticacaoProfissionalTestHelper
            .PutComoJsonProtegidoAsync(
                client,
                $"/api/fichas/{fichaId}/atendimento",
                request,
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static RegistrarAtendimentoRequest CriarRequestValido()
    {
        return new RegistrarAtendimentoRequest
        {
            DataRealizacao = DateOnly.FromDateTime(DateTime.UtcNow),
            ValorCobrado = 200m,
            Desconto = 20m,
            FormaPagamento = FormaPagamento.Pix
        };
    }

    private static async Task<Guid> CriarFichaAsync(
        FichaDigitalApiFactory factory,
        bool concluida)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var cliente = new Cliente("Ana");
        var ficha = new Ficha(cliente.Id);

        if (concluida)
        {
            ficha.EnviarConvite();
            ficha.IniciarPreenchimento();
            ficha.Concluir();
        }

        dbContext.Clientes.Add(cliente);
        dbContext.Fichas.Add(ficha);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        return ficha.Id;
    }
}
