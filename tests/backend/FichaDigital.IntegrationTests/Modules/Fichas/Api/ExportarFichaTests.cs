using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Api;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.IntegrationTests.Modules.Fichas.Api;

public sealed class ExportarFichaTests
{
    [Fact]
    public async Task Exportar_SemSessao_NaoPermiteAcesso()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"/api/fichas/{Guid.NewGuid()}/exportacao-pdf", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Exportar_PreservaTermoEDadosHistoricosERegistraSolicitacao()
    {
        var agora = DateTimeOffset.UtcNow;
        using var factory = new FichaDigitalApiFactory(new FixedTimeProvider(agora));
        var id = await ObterDetalheFichaTests.CriarFichaConcluidaAsync(factory, agora, evidenciaIntegra: true);
        using var client = await AutenticacaoProfissionalTestHelper.CriarClienteAutenticadoAsync(factory, TestContext.Current.CancellationToken);
        using var response = await client.GetAsync($"/api/fichas/{id}/exportacao-pdf", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);
        var dados = await response.Content.ReadFromJsonAsync<ExportacaoFichaResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(dados);
        Assert.Equal("Termo fictício para teste automatizado.", dados.ConteudoTermo);
        Assert.Equal("ana@example.com", dados.Ficha.Cliente.Email);
        Assert.Equal(agora, dados.ExportadaEmUtc);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FichaDigitalDbContext>();
        var registro = await db.RegistrosAuditoria.SingleAsync(item => item.RecursoId == id && item.Acao.Contains("exportacao-pdf"), TestContext.Current.CancellationToken);
        Assert.NotNull(registro.ProfissionalId);
        Assert.Equal(agora, registro.OcorreuEmUtc);
    }

    [Fact]
    public async Task Exportar_EvidenciaAlterada_RetornaConflict()
    {
        using var factory = new FichaDigitalApiFactory();
        var id = await ObterDetalheFichaTests.CriarFichaConcluidaAsync(factory, DateTimeOffset.UtcNow);
        using var client = await AutenticacaoProfissionalTestHelper.CriarClienteAutenticadoAsync(factory, TestContext.Current.CancellationToken);
        using var response = await client.GetAsync($"/api/fichas/{id}/exportacao-pdf", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Exportar_FichaNaoConcluidaOuIncompleta_RetornaConflict(bool concluir)
    {
        using var factory = new FichaDigitalApiFactory();
        Guid id;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FichaDigitalDbContext>();
            var cliente = new Cliente(DadosPessoaisTeste.Criar());
            var ficha = new Ficha(cliente.Id);
            if (concluir) { ficha.EnviarConvite(); ficha.IniciarPreenchimento(); ficha.ConcluirFluxoLegado(); }
            db.Clientes.Add(cliente);
            db.Fichas.Add(ficha);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            id = ficha.Id;
        }
        using var client = await AutenticacaoProfissionalTestHelper.CriarClienteAutenticadoAsync(factory, TestContext.Current.CancellationToken);
        using var response = await client.GetAsync($"/api/fichas/{id}/exportacao-pdf", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Exportar_FichaInexistente_RetornaNotFound()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = await AutenticacaoProfissionalTestHelper.CriarClienteAutenticadoAsync(factory, TestContext.Current.CancellationToken);
        using var response = await client.GetAsync($"/api/fichas/{Guid.NewGuid()}/exportacao-pdf", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
