using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure;
using Microsoft.Extensions.Options;

namespace FichaDigital.UnitTests.Modules.Fichas.Application;

public sealed class EmitirConviteFichaServiceTests
{
    [Fact]
    public async Task Emitir_ComClienteExistente_DeveCriarFichaEConvite()
    {
        var instanteEmissao = DateTimeOffset.UtcNow.AddMinutes(1);
        var repository = new EmissaoConviteRepositorySpy(clienteExiste: true);
        var geradorToken = new GeradorTokenConviteStub();
        var service = CriarService(
            repository,
            geradorToken,
            instanteEmissao);
        var clienteId = Guid.NewGuid();
        var profissionalId = Guid.NewGuid();

        var resultado = await service.EmitirAsync(
            clienteId,
            profissionalId,
            "Profissional responsável",
            TipoProcedimento.Tatuagem,
            TestContext.Current.CancellationToken);

        Assert.NotNull(resultado);
        var ficha = Assert.IsType<Ficha>(repository.FichaAdicionada);
        var convite = Assert.IsType<ConviteFicha>(
            repository.ConviteAdicionado);
        Assert.Equal(clienteId, ficha.ClienteId);
        Assert.Equal(profissionalId, ficha.ProfissionalResponsavelId);
        Assert.Equal(StatusFicha.ConviteEnviado, ficha.Status);
        Assert.Equal(ficha.Id, convite.FichaId);
        Assert.Equal(geradorToken.TokenOriginal, resultado.TokenOriginal);
        Assert.Equal(geradorToken.TokenHash, convite.TokenHash);
        Assert.Equal(instanteEmissao.AddHours(1), convite.ExpiraEmUtc);
        Assert.Equal(convite.Id, resultado.ConviteId);
    }

    [Fact]
    public async Task Emitir_ComClienteInexistente_NaoDevePersistir()
    {
        var repository = new EmissaoConviteRepositorySpy(
            clienteExiste: false);
        var geradorToken = new GeradorTokenConviteStub();
        var service = CriarService(
            repository,
            geradorToken,
            DateTimeOffset.UtcNow.AddMinutes(1));

        var resultado = await service.EmitirAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Profissional responsável",
            TipoProcedimento.Piercing,
            TestContext.Current.CancellationToken);

        Assert.Null(resultado);
        Assert.Null(repository.FichaAdicionada);
        Assert.Null(repository.ConviteAdicionado);
        Assert.False(geradorToken.GerarFoiChamado);
    }

    private static EmitirConviteFichaService CriarService(
        IEmissaoConviteRepository repository,
        IGeradorTokenConvite geradorToken,
        DateTimeOffset instanteEmissao)
    {
        var resolvedorModelo = new ResolvedorModeloFicha(
            Options.Create(new EstudioOptions
            {
                Cnpj = "00.000.000/0000-00"
            }));

        return new EmitirConviteFichaService(
            repository,
            geradorToken,
            resolvedorModelo,
            new FixedTimeProvider(instanteEmissao));
    }

    private sealed class GeradorTokenConviteStub : IGeradorTokenConvite
    {
        public string TokenOriginal => "token-unitario";

        public string TokenHash => new('a', 64);

        public bool GerarFoiChamado { get; private set; }

        public TokenConviteGerado Gerar()
        {
            GerarFoiChamado = true;
            return new TokenConviteGerado(TokenOriginal, TokenHash);
        }

        public string CalcularHash(string tokenOriginal)
        {
            return TokenHash;
        }
    }

    private sealed class EmissaoConviteRepositorySpy(bool clienteExiste)
        : IEmissaoConviteRepository
    {
        public Ficha? FichaAdicionada { get; private set; }

        public ConviteFicha? ConviteAdicionado { get; private set; }

        public Task<bool> ClienteExisteAsync(
            Guid clienteId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(clienteExiste);
        }

        public Task AdicionarAsync(
            Ficha ficha,
            ConviteFicha convite,
            CancellationToken cancellationToken)
        {
            FichaAdicionada = ficha;
            ConviteAdicionado = convite;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}
