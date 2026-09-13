using System.Net;
using System.Net.Http.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Api;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Modules.Fichas.Api;

public sealed class AceitarTermoConsentimentoTests
{
    private const string AssinaturaPng =
        "data:image/png;base64," +
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0l" +
        "EQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";

    [Fact]
    public async Task Aceitar_AposAnamnese_DevePersistirAceiteEAutorizarFicha()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var convite = await PrepararConviteAbertoAsync(factory, client);
        await ResponderQuestionarioAsync(client, convite.Token);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/termo-consentimento/aceitar",
            CriarRequestValido(convite),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);

        var response = await httpResponse.Content
            .ReadFromJsonAsync<TermoConsentimentoAceitoResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.AceiteId);
        Assert.Equal(convite.FichaId, response.FichaId);
        Assert.Equal(convite.Termo.Versao, response.VersaoTermo);
        Assert.Equal("AutorizadaParaProcedimento", response.StatusFicha);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var ficha = await dbContext.Fichas
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == convite.FichaId,
                TestContext.Current.CancellationToken);
        var aceite = await dbContext.AceitesTermoConsentimento
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == response.AceiteId,
                TestContext.Current.CancellationToken);

        Assert.Equal(StatusFicha.AutorizadaParaProcedimento, ficha.Status);
        Assert.Equal(convite.Termo.Conteudo, aceite.ConteudoTermo);
        Assert.Equal(convite.Termo.ConteudoHash, aceite.ConteudoHash);
        Assert.Equal("Ana Silva", aceite.NomeAssinante);
        Assert.True(aceite.ConfirmouLeituraEAutorizacao);
        Assert.Equal(AssinaturaPng, aceite.AssinaturaDesenhada);
        Assert.NotEqual(Guid.Empty, aceite.ConviteId);
        Assert.Equal(64, aceite.EvidenciaHash.Length);
        Assert.Contains("\"nomeCompleto\":\"Ana Silva\"", aceite.EvidenciaJson);
        Assert.Contains(
            "\"confirmouLeituraEAutorizacao\":true",
            aceite.EvidenciaJson);
        Assert.Equal(aceite.EvidenciaHash, response.EvidenciaHash);

        using var reabrirResponse = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest { Token = convite.Token },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, reabrirResponse.StatusCode);
    }

    [Fact]
    public async Task Aceitar_SemConfirmarLeituraEAutorizacao_DeveRetornarBadRequest()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var convite = await PrepararConviteAbertoAsync(factory, client);
        await ResponderQuestionarioAsync(client, convite.Token);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/termo-consentimento/aceitar",
            CriarRequestValido(
                convite,
                confirmouLeituraEAutorizacao: false),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        Assert.False(await dbContext.AceitesTermoConsentimento.AnyAsync(
            item => item.FichaId == convite.FichaId,
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Aceitar_SemQuestionario_DeveRetornarConflictENaoConcluirFicha()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var convite = await PrepararConviteAbertoAsync(factory, client);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/termo-consentimento/aceitar",
            CriarRequestValido(convite),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);

        var problemDetails = await httpResponse.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal("Questionário pendente.", problemDetails.Title);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var ficha = await dbContext.Fichas
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == convite.FichaId,
                TestContext.Current.CancellationToken);

        Assert.Equal(StatusFicha.EmPreenchimento, ficha.Status);
        Assert.Empty(await dbContext.AceitesTermoConsentimento.ToListAsync(
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Aceitar_ComHashDesatualizado_DeveRetornarConflict()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var convite = await PrepararConviteAbertoAsync(factory, client);
        await ResponderQuestionarioAsync(client, convite.Token);
        var request = new AceitarTermoConsentimentoRequest
        {
            Token = convite.Token,
            VersaoTermo = convite.Termo.Versao,
            ConteudoHash = new string('0', 64),
            NomeAssinante = "Ana Silva",
            ConfirmouLeituraEAutorizacao = true,
            AssinaturaDesenhada = AssinaturaPng
        };

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/termo-consentimento/aceitar",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);

        var problemDetails = await httpResponse.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal("Termo atualizado.", problemDetails.Title);
    }

    [Fact]
    public async Task FluxoTatuagem_AteRegistroTecnico_DeveConcluirFicha()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var convite = await PrepararConviteAbertoAsync(factory, client);
        using var clientProfissional = convite.ClienteProfissional;
        await ResponderQuestionarioAsync(client, convite.Token);

        using var aceiteResponse = await client.PostAsJsonAsync(
            "/api/fichas/termo-consentimento/aceitar",
            CriarRequestValido(convite),
            TestContext.Current.CancellationToken);
        aceiteResponse.EnsureSuccessStatusCode();

        using var revisaoResponse = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
                clientProfissional,
                $"/api/fichas/{convite.FichaId}/operacoes/revisar",
                new RevisarFichaRequest
                {
                    DadosDaFichaConferidos = true
                },
                TestContext.Current.CancellationToken);
        revisaoResponse.EnsureSuccessStatusCode();

        using var conclusaoResponse = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
                clientProfissional,
                $"/api/fichas/{convite.FichaId}/operacoes/concluir",
                new ConcluirProcedimentoRequest
                {
                    ValorTotal = 500m,
                    ValorSinal = 100m,
                    FormaPagamento = FormaPagamento.Pix,
                    AssinaturaDesenhada = AssinaturaPng,
                    Tatuagem = new RegistroTatuagemRequest
                    {
                        ArteEfetivamenteTatuada = "Arte floral aprovada",
                        MaterialUtilizado = "Agulha 3RL e tinta preta",
                        LocalTatuagem = "Antebraço direito",
                        Observacoes = "Procedimento sem intercorrências"
                    }
                },
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, conclusaoResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var ficha = await dbContext.Fichas.AsNoTracking().SingleAsync(
            item => item.Id == convite.FichaId,
            TestContext.Current.CancellationToken);
        var revisao = await dbContext.RevisoesProfissionais.AsNoTracking()
            .SingleAsync(
                item => item.FichaId == convite.FichaId,
                TestContext.Current.CancellationToken);
        var registro = await dbContext.RegistrosTatuagem.AsNoTracking()
            .SingleAsync(
                item => item.FichaId == convite.FichaId,
                TestContext.Current.CancellationToken);

        Assert.Equal(StatusFicha.Concluida, ficha.Status);
        Assert.True(revisao.DadosDaFichaConferidos);
        Assert.Equal("Lia Tatuadora", revisao.ProfissionalNome);
        Assert.Equal("Lia Tatuadora", registro.ProfissionalNome);
        Assert.Equal("Lia Tatuadora", registro.NomeProfissionalAssinante);
        Assert.Equal(400m, registro.ValorRestante);
        Assert.Equal(64, registro.EvidenciaHash.Length);
    }

    [Fact]
    public async Task FluxoPiercing_AteRegistroTecnico_DeveConcluirFicha()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var convite = await PrepararConviteAbertoAsync(
            factory,
            client,
            TipoProcedimento.Piercing);
        using var clientProfissional = convite.ClienteProfissional;
        await AutorizarERevisarAsync(client, convite);

        using var conclusaoResponse = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
                clientProfissional,
                $"/api/fichas/{convite.FichaId}/operacoes/concluir",
                new ConcluirProcedimentoRequest
                {
                    ValorTotal = 180m,
                    ValorSinal = 50m,
                    FormaPagamento = FormaPagamento.Cartao,
                    AssinaturaDesenhada = AssinaturaPng,
                    Piercing = new RegistroPiercingRequest
                    {
                        JoiaUtilizada = "Titânio grau implante",
                        AgulhaUtilizada = "Agulha americana 14G",
                        LocalPerfuracao = "Hélix esquerdo"
                    }
                },
                TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, conclusaoResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var registro = await dbContext.RegistrosPiercing.AsNoTracking()
            .SingleAsync(
                item => item.FichaId == convite.FichaId,
                TestContext.Current.CancellationToken);

        Assert.Equal("Titânio grau implante", registro.JoiaUtilizada);
        Assert.Equal(130m, registro.ValorRestante);
    }

    private static async Task AutorizarERevisarAsync(
        HttpClient client,
        ConviteAberto convite)
    {
        await ResponderQuestionarioAsync(client, convite.Token);

        using var aceiteResponse = await client.PostAsJsonAsync(
            "/api/fichas/termo-consentimento/aceitar",
            CriarRequestValido(convite),
            TestContext.Current.CancellationToken);
        aceiteResponse.EnsureSuccessStatusCode();

        using var revisaoResponse = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
                convite.ClienteProfissional,
                $"/api/fichas/{convite.FichaId}/operacoes/revisar",
                new RevisarFichaRequest
                {
                    DadosDaFichaConferidos = true
                },
                TestContext.Current.CancellationToken);
        revisaoResponse.EnsureSuccessStatusCode();
    }

    private static AceitarTermoConsentimentoRequest CriarRequestValido(
        ConviteAberto convite,
        bool confirmouLeituraEAutorizacao = true)
    {
        return new AceitarTermoConsentimentoRequest
        {
            Token = convite.Token,
            VersaoTermo = convite.Termo.Versao,
            ConteudoHash = convite.Termo.ConteudoHash,
            NomeAssinante = "  Ana Silva  ",
            ConfirmouLeituraEAutorizacao = confirmouLeituraEAutorizacao,
            AssinaturaDesenhada = AssinaturaPng
        };
    }

    private static async Task<ConviteAberto> PrepararConviteAbertoAsync(
        FichaDigitalApiFactory factory,
        HttpClient client,
        TipoProcedimento tipoProcedimento = TipoProcedimento.Tatuagem)
    {
        var clienteId = await CriarClienteAsync(factory);
        var clientProfissional = await AutenticacaoProfissionalTestHelper
            .CriarClienteAutenticadoAsync(
                factory,
                TestContext.Current.CancellationToken);
        using var emitirResponse = await AutenticacaoProfissionalTestHelper
            .PostComoJsonProtegidoAsync(
            clientProfissional,
            $"/api/clientes/{clienteId}/fichas/convites",
            new EmitirConviteFichaRequest
            {
                ProfissionalResponsavelNome = "Lia Tatuadora",
                TipoProcedimento = tipoProcedimento
            },
            TestContext.Current.CancellationToken);
        emitirResponse.EnsureSuccessStatusCode();

        var conviteCriado = (await emitirResponse.Content
            .ReadFromJsonAsync<ConviteFichaCriadoResponse>(
                TestContext.Current.CancellationToken))!;
        var token = conviteCriado.LinkPreenchimento.Split('/').Last();

        using var abrirResponse = await client.PostAsJsonAsync(
            "/api/fichas/convites/abrir",
            new AbrirConviteFichaRequest
            {
                Token = token
            },
            TestContext.Current.CancellationToken);
        abrirResponse.EnsureSuccessStatusCode();

        var conviteAberto = (await abrirResponse.Content
            .ReadFromJsonAsync<ConviteFichaAbertoResponse>(
                TestContext.Current.CancellationToken))!;

        using var dadosResponse = await client.PostAsJsonAsync(
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
        dadosResponse.EnsureSuccessStatusCode();

        return new ConviteAberto(
            token,
            conviteAberto.FichaId,
            conviteAberto.TermoConsentimento,
            tipoProcedimento,
            clientProfissional);
    }

    private static async Task ResponderQuestionarioAsync(
        HttpClient client,
        string token)
    {
        using var response = await client.PostAsJsonAsync(
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

        response.EnsureSuccessStatusCode();
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

    private sealed record ConviteAberto(
        string Token,
        Guid FichaId,
        TermoConsentimentoResponse Termo,
        TipoProcedimento TipoProcedimento,
        HttpClient ClienteProfissional);
}
