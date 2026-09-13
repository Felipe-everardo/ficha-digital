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

public sealed class PreencherDadosPessoaisTests
{
    [Fact]
    public async Task Preencher_ComDadosValidos_DeveAtualizarClienteECriarRetratoDaFicha()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var (token, fichaId, clienteId) = await PrepararConviteAbertoAsync(factory);

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/dados-pessoais",
            CriarRequestValido(token),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

        var response = await httpResponse.Content
            .ReadFromJsonAsync<DadosPessoaisPreenchidosResponse>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(fichaId, response.FichaId);
        Assert.Equal(clienteId, response.ClienteId);
        Assert.Equal("Ana", response.NomeParaExibicao);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == clienteId,
                TestContext.Current.CancellationToken);
        var dadosDaFicha = await dbContext.DadosPessoaisFichas
            .AsNoTracking()
            .SingleAsync(
                item => item.FichaId == fichaId,
                TestContext.Current.CancellationToken);

        Assert.Equal("Ana Silva", cliente.NomeCompleto);
        Assert.Equal("@ana", cliente.Instagram);
        Assert.Equal("Maria", cliente.ContatoEmergenciaNome);
        Assert.Equal("(21) 98888-8888", cliente.ContatoEmergenciaCelular);
        Assert.True(cliente.DadosPessoaisPreenchidos);
        Assert.Equal("Ana Silva", dadosDaFicha.NomeCompleto);
        Assert.Equal("Ana", dadosDaFicha.NomeSocial);
        Assert.Equal("ana@example.com", dadosDaFicha.Email);
    }

    [Fact]
    public async Task Preencher_DadosJaExistentes_DevePermitirConfirmarEAtualizar()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var (token, fichaId, _) = await PrepararConviteAbertoAsync(factory);
        var request = CriarRequestValido(token);

        using var primeiraResposta = await client.PostAsJsonAsync(
            "/api/fichas/dados-pessoais",
            request,
            TestContext.Current.CancellationToken);
        var atualizacao = CriarRequestValido(
            token,
            "(21) 97777-7777",
            "novo-email@example.com");
        using var segundaResposta = await client.PostAsJsonAsync(
            "/api/fichas/dados-pessoais",
            atualizacao,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, primeiraResposta.StatusCode);
        Assert.Equal(HttpStatusCode.OK, segundaResposta.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var cliente = await dbContext.Clientes.AsNoTracking().SingleAsync(
            TestContext.Current.CancellationToken);
        var dadosDaFicha = await dbContext.DadosPessoaisFichas
            .AsNoTracking()
            .SingleAsync(
                item => item.FichaId == fichaId,
                TestContext.Current.CancellationToken);
        Assert.Equal("(21) 97777-7777", cliente.Celular);
        Assert.Equal("novo-email@example.com", cliente.Email);
        Assert.Equal("(21) 97777-7777", dadosDaFicha.Celular);
        Assert.Equal("novo-email@example.com", dadosDaFicha.Email);
    }

    [Fact]
    public async Task Preencher_ComContatoEmergenciaIncompleto_DeveRetornarBadRequest()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var request = new PreencherDadosPessoaisRequest
        {
            Token = "token-nao-utilizado",
            NomeCompleto = "Ana Silva",
            EstadoCivil = "Solteira",
            DataNascimento = new DateOnly(1995, 6, 15),
            Cpf = "529.982.247-25",
            Celular = "(21) 99999-9999",
            ContatoEmergenciaNome = "Maria",
            ContatoEmergenciaCelular = null,
            Cep = "20040-002",
            Logradouro = "Rua da Assembleia",
            Numero = "10",
            Bairro = "Centro",
            Cidade = "Rio de Janeiro",
            Estado = "RJ"
        };

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/dados-pessoais",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact]
    public async Task Preencher_ComClienteMenorDeIdade_DeveRetornarUnprocessableEntity()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var (token, _, _) = await PrepararConviteAbertoAsync(factory);
        var request = CriarRequestValido(
            token,
            dataNascimento: DateOnly.FromDateTime(DateTime.Today).AddYears(-17));

        using var httpResponse = await client.PostAsJsonAsync(
            "/api/fichas/dados-pessoais",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.UnprocessableEntity,
            httpResponse.StatusCode);

        var problem = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(problem);
        Assert.Equal("Atendimento indisponível.", problem.Title);
    }

    [Fact]
    public async Task ResponderQuestionario_ComDadosPessoaisPendentes_DeveRetornarConflict()
    {
        using var factory = new FichaDigitalApiFactory();
        using var client = CriarHttpClient(factory);
        var (token, _, _) = await PrepararConviteAbertoAsync(factory);

        using var httpResponse = await client.PostAsJsonAsync(
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

        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);

        var problem = await httpResponse.Content
            .ReadFromJsonAsync<ProblemDetails>(
                TestContext.Current.CancellationToken);

        Assert.NotNull(problem);
        Assert.Equal("Dados pessoais pendentes.", problem.Title);
    }

    private static PreencherDadosPessoaisRequest CriarRequestValido(
        string token,
        string celular = "  (21) 99999-9999  ",
        string email = "  ana@example.com  ",
        DateOnly? dataNascimento = null)
    {
        return new PreencherDadosPessoaisRequest
        {
            Token = token,
            NomeCompleto = "  Ana Silva  ",
            NomeSocial = "  Ana  ",
            Pronomes = "  ela/dela  ",
            EstadoCivil = "  Solteira  ",
            DataNascimento = dataNascimento ?? new DateOnly(1995, 6, 15),
            Cpf = "529.982.247-25",
            Celular = celular,
            Email = email,
            Instagram = "  @ana  ",
            ContatoEmergenciaNome = "  Maria  ",
            ContatoEmergenciaCelular = "  (21) 98888-8888  ",
            Cep = "20040-002",
            Logradouro = "  Rua da Assembleia  ",
            Numero = "10",
            Bairro = "  Centro  ",
            Cidade = "  Rio de Janeiro  ",
            Estado = "rj"
        };
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

    private static async Task<(string Token, Guid FichaId, Guid ClienteId)>
        PrepararConviteAbertoAsync(FichaDigitalApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var geradorToken = scope.ServiceProvider
            .GetRequiredService<GeradorTokenConvite>();
        var timeProvider = scope.ServiceProvider
            .GetRequiredService<TimeProvider>();
        var cliente = new Cliente("Ana");
        var ficha = new Ficha(cliente.Id);
        ficha.EnviarConvite();
        ficha.IniciarPreenchimento();
        var token = geradorToken.Gerar();
        var convite = new ConviteFicha(
            ficha.Id,
            token.TokenHash,
            timeProvider.GetUtcNow().AddHours(1));

        dbContext.Clientes.Add(cliente);
        dbContext.Fichas.Add(ficha);
        dbContext.ConvitesFicha.Add(convite);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        return (token.TokenOriginal, ficha.Id, cliente.Id);
    }
}
