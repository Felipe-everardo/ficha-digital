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

public sealed class AbrirConviteFichaTests
{
    [Fact]
    public async Task Abrir_ComConviteValido_DeveIniciarPreenchimento()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var conviteEmitido = await EmitirConviteAsync(factory);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = ObterToken(conviteEmitido.LinkPreenchimento)
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

        var response = await httpResponse.Content
            .ReadFromJsonAsync<ConviteFichaAbertoResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(conviteEmitido.FichaId, response.FichaId);
        Assert.Equal("EmPreenchimento", response.Status);
        Assert.False(response.QuestionarioRespondido);
        Assert.False(response.DadosPessoaisPreenchidos);
        Assert.Equal("Tatuagem", response.TipoProcedimento);
        Assert.Equal("Ana Silva", response.DadosPessoais.NomeCompleto);
        Assert.Equal("(21) 99999-9999", response.DadosPessoais.Celular);
        Assert.Equal(1, response.TermoConsentimento.Versao);
        Assert.Contains(
            "TERMO DE AUTORIZAÇÃO DE TATUAGEM",
            response.TermoConsentimento.Conteudo);
        Assert.Contains(
            "00.000.000/0000-00",
            response.TermoConsentimento.Conteudo);
        Assert.Equal(
            "Lia Tatuadora",
            response.ProfissionalResponsavelNome);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var ficha = await dbContext.Fichas
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == response.FichaId,
                TestContext.Current.CancellationToken);

        Assert.Equal(StatusFicha.EmPreenchimento, ficha.Status);
        Assert.Equal(1, ficha.VersaoModelo);
        Assert.Equal(QuestionarioSaude.VersaoAtual, ficha.VersaoQuestionario);
        Assert.Equal(1, ficha.VersaoTermo);
        Assert.Equal("00.000.000/0000-00", ficha.CnpjApresentado);

        var auditoria = await dbContext.RegistrosAuditoria
            .AsNoTracking()
            .SingleAsync(
                registro =>
                    registro.RecursoId == response.FichaId &&
                    registro.Acao == "Convite aberto",
                TestContext.Current.CancellationToken);
        Assert.Equal("Cliente", auditoria.Origem);
        Assert.Equal("Ficha", auditoria.Recurso);
        Assert.Null(auditoria.ProfissionalId);
        Assert.Equal(
            httpResponse.Headers.GetValues("X-Correlation-ID").Single(),
            auditoria.CorrelacaoId);
    }

    [Fact]
    public async Task Abrir_QuandoAuditoriaFalha_DeveReverterMudancaDaFicha()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var conviteEmitido = await EmitirConviteAsync(factory);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<FichaDigitalDbContext>();
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TRIGGER FalharAuditoria
                BEFORE INSERT ON RegistrosAuditoria
                BEGIN
                    SELECT RAISE(ABORT, 'falha de auditoria');
                END;
                """,
                TestContext.Current.CancellationToken);
        }

        using var response = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = ObterToken(conviteEmitido.LinkPreenchimento)
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        using var verificacaoScope = factory.Services.CreateScope();
        var verificacaoContext = verificacaoScope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var status = await verificacaoContext.Fichas
            .AsNoTracking()
            .Where(ficha => ficha.Id == conviteEmitido.FichaId)
            .Select(ficha => ficha.Status)
            .SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(StatusFicha.ConviteEnviado, status);
        Assert.False(await verificacaoContext.RegistrosAuditoria.AnyAsync(
            registro => registro.RecursoId == conviteEmitido.FichaId,
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Abrir_ConviteDePiercing_DeveRetornarTermoDePiercing()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var conviteEmitido = await EmitirConviteAsync(
            factory,
            TipoProcedimento.Piercing);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = ObterToken(conviteEmitido.LinkPreenchimento)
            },
            TestContext.Current.CancellationToken);

        var response = await httpResponse.Content
            .ReadFromJsonAsync<ConviteFichaAbertoResponse>(
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(response);
        Assert.Equal("Piercing", response.TipoProcedimento);
        Assert.Contains(
            "TERMO DE AUTORIZAÇÃO DE PIERCING",
            response.TermoConsentimento.Conteudo);
        Assert.DoesNotContain(
            "TERMO DE AUTORIZAÇÃO DE TATUAGEM",
            response.TermoConsentimento.Conteudo);
    }

    [Fact]
    public async Task Abrir_DuasVezes_DeveContinuarDisponivel()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var conviteEmitido = await EmitirConviteAsync(factory);
        var request = new AbrirConviteFichaRequest
        {
            Token = ObterToken(conviteEmitido.LinkPreenchimento)
        };

        using var primeiraResposta = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            request,
            TestContext.Current.CancellationToken);
        using var segundaResposta = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, primeiraResposta.StatusCode);
        Assert.Equal(HttpStatusCode.OK, segundaResposta.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();

        Assert.Equal(1, await dbContext.Fichas.CountAsync(
            TestContext.Current.CancellationToken));
        Assert.Equal(1, await dbContext.ConvitesFicha.CountAsync(
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Abrir_AposResponderQuestionario_DeveInformarEtapaConcluida()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var conviteEmitido = await EmitirConviteAsync(factory);
        var token = ObterToken(conviteEmitido.LinkPreenchimento);

        using var aberturaInicial = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = token
            },
            TestContext.Current.CancellationToken);

        aberturaInicial.EnsureSuccessStatusCode();

        using var respostaDados = await client.PostAsJsonAsync(
            "/api/fichas/dados-pessoais",
            new PreencherDadosPessoaisRequest
            {
                Token = token,
                NomeCompleto = "Ana Silva",
                NomeSocial = "Ana",
                Pronomes = "ela/dela",
                EstadoCivil = "Solteira",
                DataNascimento = new DateOnly(1995, 6, 15),
                Cpf = "529.982.247-25",
                Celular = "(21) 99999-9999",
                Email = "ana@example.com",
                Cep = "20040-002",
                Logradouro = "Rua da Assembleia",
                Numero = "10",
                Bairro = "Centro",
                Cidade = "Rio de Janeiro",
                Estado = "RJ"
            },
            TestContext.Current.CancellationToken);
        respostaDados.EnsureSuccessStatusCode();

        using var respostaQuestionario = await client.PostAsJsonAsync(
            "/api/fichas/questionario-saude",
            new ResponderQuestionarioSaudeRequest
            {
                Token = token,
                TemDiabetes = false,
                TeveAnemia = false,
                TeveHepatite = false,
                PossuiPressaoAlta = false,
                TemAlergia = false,
                PossuiCondicaoCardiaca = false,
                TemEpilepsia = false,
                TemHemofilia = false,
                PossuiDoencaTransmissivel = false,
                UsaMarcaPasso = false,
                Fuma = false,
                ConsumiuBebidaAlcoolicaUltimas24Horas = false,
                UsaMedicacao = false,
                EstaGravidaOuAmamentando = false
            },
            TestContext.Current.CancellationToken);

        respostaQuestionario.EnsureSuccessStatusCode();

        using var reabertura = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = token
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, reabertura.StatusCode);

        var response = await reabertura.Content
            .ReadFromJsonAsync<ConviteFichaAbertoResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.True(response.QuestionarioRespondido);
        Assert.Equal("AnamnesePreenchida", response.Status);
    }

    [Fact]
    public async Task Abrir_ComTokenInvalido_DeveRetornarNotFound()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = "token-que-nao-existe"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);

        var problemDetails = await httpResponse.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal("Convite inválido.", problemDetails.Title);
    }

    [Fact]
    public async Task Abrir_ComConviteExpirado_DeveRetornarGone()
    {
        var expiraEmUtc = DateTimeOffset.UtcNow.AddHours(1);
        var timeProvider = new FixedTimeProvider(
            expiraEmUtc.AddMinutes(1));
        using var factory = new FichaDigitalApiFactory(timeProvider);
        using var client = CriarHttpClient(factory);
        var tokenOriginal = await CriarConviteAsync(
            factory,
            expiraEmUtc);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = tokenOriginal
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Gone, httpResponse.StatusCode);

        var problemDetails = await httpResponse.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal("Convite expirado.", problemDetails.Title);
    }

    [Fact]
    public async Task Abrir_AposExcederLimite_DeveRetornarTooManyRequests()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var request = new AbrirConviteFichaRequest
        {
            Token = "token-inexistente"
        };

        for (var tentativa = 1; tentativa <= 10; tentativa++)
        {
            using var respostaPermitida = await client.PostAsJsonAsync(
                "/api/fichas/convites/abrir",
                request,
                TestContext.Current.CancellationToken);

            Assert.Equal(
                HttpStatusCode.NotFound,
                respostaPermitida.StatusCode);
        }

        using var respostaBloqueada = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            respostaBloqueada.StatusCode);

        var problemDetails = await respostaBloqueada.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal("Muitas tentativas.", problemDetails.Title);
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

    private static async Task<ConviteFichaCriadoResponse> EmitirConviteAsync(
        FichaDigitalApiFactory factory,
        TipoProcedimento tipoProcedimento = TipoProcedimento.Tatuagem)
    {
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
                TipoProcedimento = tipoProcedimento
            },
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        return (await response.Content
            .ReadFromJsonAsync<ConviteFichaCriadoResponse>(
                TestContext.Current.CancellationToken))!;
    }

    private static string ObterToken(string linkPreenchimento)
    {
        return linkPreenchimento.Split('/').Last();
    }

    private static async Task<Guid> CriarClienteAsync(
        FichaDigitalApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var cliente = CriarCliente();

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        return cliente.Id;
    }

    private static async Task<string> CriarConviteAsync(
        FichaDigitalApiFactory factory,
        DateTimeOffset expiraEmUtc)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var geradorToken = scope.ServiceProvider
            .GetRequiredService<GeradorTokenConvite>();
        var cliente = CriarCliente();
        var ficha = new Ficha(cliente.Id);
        ficha.EnviarConvite();
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

    private static Cliente CriarCliente()
    {
        return new Cliente(DadosPessoaisTeste.Criar(
            celular: "(21) 99999-9999"));
    }
}
