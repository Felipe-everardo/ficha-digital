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

public sealed class ObterDetalheFichaTests
{
    [Fact]
    public async Task ObterDetalhe_SemAutenticacao_DeveRetornarUnauthorized()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        using var response = await client.GetAsync(
            $"/api/fichas/{Guid.NewGuid()}",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ObterDetalhe_ComFichaInexistente_DeveRetornarNotFound()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await client.GetAsync(
            $"/api/fichas/{Guid.NewGuid()}",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ObterDetalhe_ComSessaoValida_DeveRetornarDadosSemSegredos()
    {
        var agora = DateTimeOffset.UtcNow;
        using var factory = new FichaDigitalApiFactory(
            new FixedTimeProvider(agora));
        var fichaId = await CriarFichaConcluidaAsync(factory, agora);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await client.GetAsync(
            $"/api/fichas/{fichaId}",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);

        var corpo = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);
        var detalhe = await response.Content
            .ReadFromJsonAsync<FichaDetalheResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(detalhe);
        Assert.Equal(fichaId, detalhe.Id);
        Assert.Equal("Concluida", detalhe.Status);
        Assert.Equal("Ana", detalhe.Cliente.NomeParaExibicao);
        Assert.Equal(new DateOnly(1995, 6, 15), detalhe.Cliente.DataNascimento);

        Assert.NotNull(detalhe.QuestionarioSaude);
        Assert.True(detalhe.QuestionarioSaude.TemDiabetes);
        Assert.Equal("Tipo 1", detalhe.QuestionarioSaude.TipoDiabetes);
        Assert.True(detalhe.QuestionarioSaude.TemAlergia);
        Assert.Equal("Látex", detalhe.QuestionarioSaude.DescricaoAlergia);

        Assert.NotNull(detalhe.AceiteTermo);
        Assert.Equal("Ana Silva", detalhe.AceiteTermo.NomeAssinante);
        Assert.Equal(1, detalhe.AceiteTermo.VersaoTermo);

        Assert.DoesNotContain("tokenHash", corpo);
        Assert.DoesNotContain("conteudoTermo", corpo);
        Assert.DoesNotContain("conteudoHash", corpo);
    }

    private static async Task<Guid> CriarFichaConcluidaAsync(
        FichaDigitalApiFactory factory,
        DateTimeOffset agora)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var cliente = new Cliente(
            "Ana Silva",
            "Ana",
            "ela/dela",
            new DateOnly(1995, 6, 15),
            "21911111111",
            "ana@example.com");
        var ficha = new Ficha(cliente.Id);
        ficha.EnviarConvite();
        ficha.IniciarPreenchimento();
        ficha.Concluir();
        var convite = new ConviteFicha(
            ficha.Id,
            new string('c', 64),
            agora.AddHours(5));
        var questionario = new QuestionarioSaude(
            ficha.Id,
            temDiabetes: true,
            tipoDiabetes: "Tipo 1",
            possuiPressaoAlta: false,
            temAlergia: true,
            descricaoAlergia: "Látex",
            possuiCondicaoCardiaca: false,
            temEpilepsia: false,
            temHemofilia: false,
            usaMarcaPasso: false,
            estaGravidaOuAmamentando: false);
        var aceite = new AceiteTermoConsentimento(
            ficha.Id,
            versaoTermo: 1,
            conteudoTermo: "Termo fictício para teste automatizado.",
            conteudoHash: new string('d', 64),
            nomeAssinante: "Ana Silva",
            aceitoEmUtc: agora);

        dbContext.Clientes.Add(cliente);
        dbContext.Fichas.Add(ficha);
        dbContext.ConvitesFicha.Add(convite);
        dbContext.QuestionariosSaude.Add(questionario);
        dbContext.AceitesTermoConsentimento.Add(aceite);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        return ficha.Id;
    }
}
