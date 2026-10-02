using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.UnitTests.Modules.Fichas.Application;

public sealed class RevisarFichaServiceTests
{
    private const string Signature = "data:image/png;base64," +
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0l" +
        "EQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";
    private static readonly DateTimeOffset Now = new(2026, 10, 2, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Revisar_ComConsentimentoValido_PersisteRevisaoETransicaoJuntas()
    {
        var ficha = CriarFichaAutorizada();
        var repository = new Repository(ficha, CriarAceite(ficha.Id));
        var service = new RevisarFichaService(repository, new HashStub(), new Clock());

        var result = await service.RevisarAsync(ficha.Id, ficha.ProfissionalResponsavelId!.Value,
            true, TestContext.Current.CancellationToken);

        Assert.Equal(StatusRevisaoFicha.Confirmada, result.Resultado);
        Assert.Equal(StatusFicha.RevisadaPeloProfissional, ficha.Status);
        Assert.Same(ficha, repository.FichaSalva);
        Assert.NotNull(repository.Revisao);
        Assert.Equal(ficha.Id, repository.Revisao.FichaId);
        Assert.Equal(Now, repository.Revisao.RevisadaEmUtc);
        Assert.Equal(1, repository.Gravacoes);
    }

    [Theory]
    [InlineData("outro-profissional", StatusRevisaoFicha.ProfissionalNaoResponsavel)]
    [InlineData("nao-conferida", StatusRevisaoFicha.DadosNaoConferidos)]
    [InlineData("sem-aceite", StatusRevisaoFicha.ConsentimentoInvalido)]
    [InlineData("evidencia-alterada", StatusRevisaoFicha.ConsentimentoInvalido)]
    [InlineData("sem-assinatura", StatusRevisaoFicha.ConsentimentoInvalido)]
    [InlineData("ja-revisada", StatusRevisaoFicha.FichaIndisponivel)]
    public async Task Revisar_ComPrecondicaoInvalida_NaoPersiste(string scenario, StatusRevisaoFicha expected)
    {
        var ficha = CriarFichaAutorizada();
        if (scenario == "ja-revisada") ficha.ConfirmarRevisaoProfissional();
        var original = ficha.Status;
        var version = ficha.VersaoConcorrencia;
        var aceite = scenario == "sem-aceite" ? null : CriarAceite(ficha.Id,
            validHash: scenario != "evidencia-alterada", signed: scenario != "sem-assinatura");
        var repository = new Repository(ficha, aceite);
        var service = new RevisarFichaService(repository, new HashStub(), new Clock());

        var result = await service.RevisarAsync(ficha.Id,
            scenario == "outro-profissional" ? Guid.NewGuid() : ficha.ProfissionalResponsavelId!.Value,
            scenario != "nao-conferida", TestContext.Current.CancellationToken);

        Assert.Equal(expected, result.Resultado);
        Assert.Equal(original, ficha.Status);
        Assert.Equal(version, ficha.VersaoConcorrencia);
        Assert.Equal(0, repository.Gravacoes);
    }

    [Fact]
    public async Task Revisar_ComFichaInexistente_NaoPersiste()
    {
        var repository = new Repository(null, null);
        var service = new RevisarFichaService(repository, new HashStub(), new Clock());
        var result = await service.RevisarAsync(Guid.NewGuid(), Guid.NewGuid(), true,
            TestContext.Current.CancellationToken);
        Assert.Equal(StatusRevisaoFicha.FichaNaoEncontrada, result.Resultado);
        Assert.Equal(0, repository.Gravacoes);
    }

    private static Ficha CriarFichaAutorizada()
    {
        var ficha = new Ficha(Guid.NewGuid(), Guid.NewGuid(), "Profissional", TipoProcedimento.Tatuagem);
        ficha.EnviarConvite();
        ficha.IniciarPreenchimento();
        ficha.ConcluirAnamnese();
        ficha.AutorizarProcedimento();
        return ficha;
    }

    private static AceiteTermoConsentimento CriarAceite(Guid fichaId, bool validHash = true, bool signed = true) =>
        new(fichaId, null, 1, "Termo de teste", new string('a', 64), "Cliente fictício", true,
            1, "{}", new string(validHash ? 'a' : 'b', 64), null, null, Now, signed ? Signature : null);

    private sealed class HashStub : ICalculadorHashConteudo
    {
        public string Calcular(string conteudo) => new('a', 64);
    }

    private sealed class Clock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class Repository(Ficha? ficha, AceiteTermoConsentimento? aceite) : IRevisaoFichaRepository
    {
        public int Gravacoes { get; private set; }
        public Ficha? FichaSalva { get; private set; }
        public RevisaoProfissional? Revisao { get; private set; }
        public Task<Ficha?> ObterFichaAsync(Guid fichaId, CancellationToken cancellationToken) => Task.FromResult(ficha);
        public Task<AceiteTermoConsentimento?> ObterAceiteAsync(Guid fichaId, CancellationToken cancellationToken) => Task.FromResult(aceite);
        public Task SalvarRevisaoAsync(Ficha ficha, RevisaoProfissional revisao, CancellationToken cancellationToken)
        {
            FichaSalva = ficha;
            Revisao = revisao;
            Gravacoes++;
            return Task.CompletedTask;
        }
    }
}
