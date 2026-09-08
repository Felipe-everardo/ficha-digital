using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Atendimentos.Domain;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Api;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Profissionais.Domain;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Modules.Fichas.Api;

public sealed class ListarFichasTests
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
            "/api/fichas",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Listar_ComSessaoValida_DeveRetornarResumoSemDadosClinicos()
    {
        var agora = DateTimeOffset.UtcNow;
        using var factory = new FichaDigitalApiFactory(
            new FixedTimeProvider(agora.AddHours(3)));
        var fichas = await CriarFichasAsync(factory, agora);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await client.GetAsync(
            "/api/fichas?pagina=1&tamanhoPagina=10",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);
        var resultado = await response.Content
            .ReadFromJsonAsync<FichasPaginadasResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.TotalItens);
        Assert.Equal(1, resultado.TotalPaginas);
        Assert.Equal(180m, resultado.ResumoFinanceiro.TotalRecebido);
        Assert.Equal(1, resultado.ResumoFinanceiro.AtendimentosRegistrados);
        Assert.Equal(1, resultado.ResumoFinanceiro.FichasSemRegistro);

        var fichaExpirada = Assert.Single(
            resultado.Itens,
            ficha => ficha.Id == fichas.FichaExpiradaId);
        Assert.Equal("Ana", fichaExpirada.ClienteNome);
        Assert.Equal("ConviteEnviado", fichaExpirada.Status);
        Assert.True(fichaExpirada.ConviteExpirado);

        var fichaConcluida = Assert.Single(
            resultado.Itens,
            ficha => ficha.Id == fichas.FichaConcluidaId);
        Assert.Equal("Bruno Lima", fichaConcluida.ClienteNome);
        Assert.Equal("Concluida", fichaConcluida.Status);
        Assert.Equal(fichas.ConcluidaEmUtc, fichaConcluida.ConcluidaEmUtc);
        Assert.NotNull(fichaConcluida.Atendimento);
        Assert.Equal(180m, fichaConcluida.Atendimento.ValorFinal);
        Assert.False(fichaConcluida.ConviteExpirado);

        Assert.DoesNotContain("temDiabetes", corpo);
        Assert.DoesNotContain("descricaoAlergia", corpo);
        Assert.DoesNotContain("nomeAssinante", corpo);
    }

    [Fact]
    public async Task Listar_ComTamanhoAcimaDoLimite_DeveRetornarBadRequest()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await client.GetAsync(
            "/api/fichas?tamanhoPagina=51",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Listar_ComProcedimentoEStatus_DeveFiltrarAMesmaFicha()
    {
        var agora = DateTimeOffset.UtcNow;
        using var factory = new FichaDigitalApiFactory(
            new FixedTimeProvider(agora.AddHours(3)));
        await CriarFichasAsync(factory, agora);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);

        using var response = await client.GetAsync(
            "/api/fichas?tipoProcedimento=Piercing&status=Concluida",
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var resultado = await response.Content
            .ReadFromJsonAsync<FichasPaginadasResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(resultado);
        var ficha = Assert.Single(resultado.Itens);
        Assert.Equal("Bruno Lima", ficha.ClienteNome);
        Assert.Equal("Piercing", ficha.TipoProcedimento);
        Assert.Equal("Profissional do Histórico", ficha.ProfissionalResponsavelNome);
    }

    [Fact]
    public async Task Listar_ComPeriodoDeAtendimento_DeveFiltrarPelaDataDoProcedimento()
    {
        var agora = DateTimeOffset.UtcNow;
        using var factory = new FichaDigitalApiFactory(
            new FixedTimeProvider(agora.AddHours(3)));
        var fichas = await CriarFichasAsync(factory, agora);
        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);
        var data = fichas.DataRealizacao.ToString("yyyy-MM-dd");

        using var response = await client.GetAsync(
            $"/api/fichas?status=Concluida&atendimentoDe={data}" +
            $"&atendimentoAte={data}",
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var resultado = await response.Content
            .ReadFromJsonAsync<FichasPaginadasResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(resultado);
        var ficha = Assert.Single(resultado.Itens);
        Assert.Equal(fichas.FichaConcluidaId, ficha.Id);
        Assert.Equal(fichas.DataRealizacao, ficha.Atendimento?.DataRealizacao);
        Assert.Equal(180m, resultado.ResumoFinanceiro.TotalRecebido);
        Assert.Equal(0, resultado.ResumoFinanceiro.FichasSemRegistro);
    }

    [Fact]
    public async Task Listar_ComPeriodoESemRegistroFinanceiro_DeveUsarDataDeConclusao()
    {
        var concluidaEmUtc = new DateTimeOffset(
            2026,
            9,
            5,
            14,
            0,
            0,
            TimeSpan.Zero);
        using var factory = new FichaDigitalApiFactory(
            new FixedTimeProvider(concluidaEmUtc.AddHours(1)));

        Guid fichaId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<FichaDigitalDbContext>();
            var cliente = new Cliente("Cliente sem valor registrado");
            var ficha = new Ficha(cliente.Id);
            ficha.EnviarConvite();
            ficha.IniciarPreenchimento();
            ficha.Concluir();
            var aceite = new AceiteTermoConsentimento(
                ficha.Id,
                1,
                "Termo usado no teste.",
                new string('a', 64),
                "Cliente Teste",
                concluidaEmUtc);

            fichaId = ficha.Id;
            dbContext.Clientes.Add(cliente);
            dbContext.Fichas.Add(ficha);
            dbContext.AceitesTermoConsentimento.Add(aceite);
            await dbContext.SaveChangesAsync(
                TestContext.Current.CancellationToken);
        }

        using var client = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);
        using var response = await client.GetAsync(
            "/api/fichas?status=Concluida&atendimentoDe=2026-09-05" +
            "&atendimentoAte=2026-09-05",
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var resultado = await response.Content
            .ReadFromJsonAsync<FichasPaginadasResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(resultado);
        var fichaEncontrada = Assert.Single(resultado.Itens);
        Assert.Equal(fichaId, fichaEncontrada.Id);
        Assert.Null(fichaEncontrada.Atendimento);
        Assert.Equal(concluidaEmUtc, fichaEncontrada.ConcluidaEmUtc);
    }

    private static async Task<FichasCriadas> CriarFichasAsync(
        FichaDigitalApiFactory factory,
        DateTimeOffset agora)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ProfissionalUsuario>>();
        var profissional = new ProfissionalUsuario(
            "Profissional do Histórico",
            $"historico-{Guid.NewGuid():N}@example.com");
        var criacaoProfissional = await userManager.CreateAsync(profissional);
        Assert.True(criacaoProfissional.Succeeded);
        var ana = new Cliente(
            "Ana Silva",
            "Ana",
            "ela/dela",
            new DateOnly(1995, 6, 15),
            "21911111111",
            "ana@example.com");
        var bruno = new Cliente(
            "Bruno Lima",
            null,
            "ele/dele",
            new DateOnly(1990, 3, 20),
            "21922222222",
            "bruno@example.com");
        var fichaExpirada = new Ficha(
            ana.Id,
            profissional.Id,
            profissional.NomeCompleto,
            TipoProcedimento.Tatuagem);
        fichaExpirada.EnviarConvite();
        var conviteExpirado = new ConviteFicha(
            fichaExpirada.Id,
            new string('a', 64),
            agora.AddHours(1));
        var fichaConcluida = new Ficha(
            bruno.Id,
            profissional.Id,
            profissional.NomeCompleto,
            TipoProcedimento.Piercing);
        fichaConcluida.EnviarConvite();
        fichaConcluida.IniciarPreenchimento();
        fichaConcluida.Concluir();
        var conviteConcluido = new ConviteFicha(
            fichaConcluida.Id,
            new string('b', 64),
            agora.AddHours(5));
        var concluidaEmUtc = agora.AddHours(2);
        var aceite = new AceiteTermoConsentimento(
            fichaConcluida.Id,
            1,
            "Termo de consentimento usado no teste.",
            new string('c', 64),
            "Bruno Lima",
            concluidaEmUtc);
        var dataRealizacao = DateOnly.FromDateTime(agora.UtcDateTime);
        var atendimento = new Atendimento(
            fichaConcluida.Id,
            dataRealizacao,
            200m,
            20m,
            FormaPagamento.Pix,
            concluidaEmUtc.AddHours(1));

        dbContext.Clientes.AddRange(ana, bruno);
        dbContext.Fichas.AddRange(fichaExpirada, fichaConcluida);
        dbContext.ConvitesFicha.AddRange(
            conviteExpirado,
            conviteConcluido);
        dbContext.AceitesTermoConsentimento.Add(aceite);
        dbContext.Atendimentos.Add(atendimento);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        return new FichasCriadas(
            fichaExpirada.Id,
            fichaConcluida.Id,
            concluidaEmUtc,
            dataRealizacao);
    }

    private sealed record FichasCriadas(
        Guid FichaExpiradaId,
        Guid FichaConcluidaId,
        DateTimeOffset ConcluidaEmUtc,
        DateOnly DataRealizacao);
}
