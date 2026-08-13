using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.UnitTests.Modules.Fichas.Domain;

public sealed class ConviteFichaTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveArmazenarOsDados()
    {
        var fichaId = Guid.NewGuid();
        var tokenHash = new string('a', 64);
        var antesDaCriacao = DateTimeOffset.UtcNow;
        var expiraEmUtc = antesDaCriacao.AddHours(24);

        var convite = new ConviteFicha(
            fichaId,
            $"  {tokenHash}  ",
            expiraEmUtc);

        var depoisDaCriacao = DateTimeOffset.UtcNow;

        Assert.NotEqual(Guid.Empty, convite.Id);
        Assert.Equal(fichaId, convite.FichaId);
        Assert.Equal(tokenHash, convite.TokenHash);
        Assert.InRange(
            convite.CriadoEmUtc,
            antesDaCriacao,
            depoisDaCriacao);
        Assert.Equal(expiraEmUtc, convite.ExpiraEmUtc);
    }

    [Fact]
    public void Criar_ComFichaVazia_DeveLancarArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ConviteFicha(
                Guid.Empty,
                new string('a', 64),
                DateTimeOffset.UtcNow.AddHours(24)));

        Assert.Equal("fichaId", exception.ParamName);
        Assert.Contains("A ficha é obrigatória.", exception.Message);
    }

    [Fact]
    public void Criar_ComTokenHashVazio_DeveLancarArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ConviteFicha(
                Guid.NewGuid(),
                "   ",
                DateTimeOffset.UtcNow.AddHours(24)));

        Assert.Equal("tokenHash", exception.ParamName);
        Assert.Contains("O hash do token é obrigatório.", exception.Message);
    }

    [Fact]
    public void Criar_ComExpiracaoNoPassado_DeveLancarArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ConviteFicha(
                Guid.NewGuid(),
                new string('a', 64),
                DateTimeOffset.UtcNow.AddMinutes(-1)));

        Assert.Equal("expiraEmUtc", exception.ParamName);
        Assert.Contains(
            "A data de expiração deve estar no futuro.",
            exception.Message);
    }

    [Fact]
    public void EstaExpirado_DeveConsiderarOInstanteDaExpiracaoComoExpirado()
    {
        var expiraEmUtc = DateTimeOffset.UtcNow.AddHours(24);
        var convite = new ConviteFicha(
            Guid.NewGuid(),
            new string('a', 64),
            expiraEmUtc);

        Assert.False(convite.EstaExpirado(expiraEmUtc.AddTicks(-1)));
        Assert.True(convite.EstaExpirado(expiraEmUtc));
        Assert.True(convite.EstaExpirado(expiraEmUtc.AddTicks(1)));
    }
}
