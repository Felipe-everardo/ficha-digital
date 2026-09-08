using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Atendimentos.Domain;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Financeiro.Api;
using FichaDigital.Api.Modules.Financeiro.Domain;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Modules.Financeiro.Api;

public sealed class FinanceiroTests
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
            "/api/financeiro",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Despesa_ComDadosValidos_DeveCriarEAtualizar()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);
        var request = CriarRequestValido(75m);

        using var responseCriacao = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
                client,
                "/api/financeiro/despesas",
                request,
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, responseCriacao.StatusCode);
        var criada = await responseCriacao.Content
            .ReadFromJsonAsync<DespesaResponse>(
                TestContext.Current.CancellationToken);
        Assert.NotNull(criada);
        Assert.Equal(75m, criada.Valor);
        Assert.Equal("Profissional de Teste", criada.ProfissionalNome);

        var atualizacao = new SalvarDespesaRequest
        {
            Data = request.Data,
            Categoria = CategoriaDespesa.Manutencao,
            Descricao = "Manutenção da cadeira",
            Valor = 120m
        };
        using var responseAtualizacao = await AutenticacaoProfissionalTestHelper
            .PutComoJsonProtegidoAsync(
                client,
                $"/api/financeiro/despesas/{criada.Id}",
                atualizacao,
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, responseAtualizacao.StatusCode);
        var atualizada = await responseAtualizacao.Content
            .ReadFromJsonAsync<DespesaResponse>(
                TestContext.Current.CancellationToken);
        Assert.NotNull(atualizada);
        Assert.Equal(criada.Id, atualizada.Id);
        Assert.Equal("Manutencao", atualizada.Categoria);
        Assert.Equal(120m, atualizada.Valor);
    }

    [Fact]
    public async Task Listar_DeveCalcularSaldoEAplicarPeriodo()
    {
        var hoje = new DateOnly(2026, 9, 7);
        var agora = new DateTimeOffset(2026, 9, 7, 15, 0, 0, TimeSpan.Zero);
        using var factory = new FichaDigitalApiFactory(
            new FixedTimeProvider(agora));
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        await SemearMovimentacoesAsync(factory, hoje, agora);

        using var response = await client.GetAsync(
            $"/api/financeiro?dataDe={hoje:yyyy-MM-dd}&dataAte={hoje:yyyy-MM-dd}",
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var financeiro = await response.Content
            .ReadFromJsonAsync<FinanceiroResponse>(
                TestContext.Current.CancellationToken);
        Assert.NotNull(financeiro);
        Assert.Equal(400m, financeiro.Resumo.TotalRecebido);
        Assert.Equal(80m, financeiro.Resumo.TotalSaidas);
        Assert.Equal(320m, financeiro.Resumo.Saldo);
        Assert.Equal(2, financeiro.Resumo.AtendimentosPagos);
        Assert.Single(financeiro.Despesas);
    }

    [Fact]
    public async Task CriarDespesa_ComValorInvalido_DeveRetornarBadRequest()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
                client,
                "/api/financeiro/despesas",
                CriarRequestValido(0m),
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static SalvarDespesaRequest CriarRequestValido(decimal valor)
    {
        return new SalvarDespesaRequest
        {
            Data = DateOnly.FromDateTime(DateTime.UtcNow),
            Categoria = CategoriaDespesa.Materiais,
            Descricao = "Materiais descartáveis",
            Valor = valor
        };
    }

    private static async Task SemearMovimentacoesAsync(
        FichaDigitalApiFactory factory,
        DateOnly hoje,
        DateTimeOffset agora)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var profissional = await dbContext.Users.SingleAsync(
            TestContext.Current.CancellationToken);
        var cliente = new Cliente("Cliente Financeiro");
        var fichaPaga = new Ficha(cliente.Id);
        var fichaPendente = new Ficha(cliente.Id);
        var fichaForaDoPeriodo = new Ficha(cliente.Id);

        dbContext.Clientes.Add(cliente);
        dbContext.Fichas.AddRange(
            fichaPaga,
            fichaPendente,
            fichaForaDoPeriodo);
        dbContext.Atendimentos.AddRange(
            new Atendimento(
                fichaPaga.Id,
                hoje,
                300m,
                0m,
                FormaPagamento.Pix,
                agora),
            new Atendimento(
                fichaPendente.Id,
                hoje,
                100m,
                0m,
                FormaPagamento.CartaoCredito,
                agora),
            new Atendimento(
                fichaForaDoPeriodo.Id,
                hoje.AddDays(-1),
                900m,
                0m,
                FormaPagamento.Dinheiro,
                agora));
        dbContext.Despesas.AddRange(
            new Despesa(
                hoje,
                CategoriaDespesa.Contas,
                "Energia",
                80m,
                profissional.Id,
                profissional.NomeCompleto,
                agora),
            new Despesa(
                hoje.AddDays(-1),
                CategoriaDespesa.Materiais,
                "Materiais antigos",
                50m,
                profissional.Id,
                profissional.NomeCompleto,
                agora));
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);
    }
}
