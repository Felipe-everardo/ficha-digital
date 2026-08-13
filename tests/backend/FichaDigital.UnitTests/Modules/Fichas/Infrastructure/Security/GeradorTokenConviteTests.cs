using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;

namespace FichaDigital.UnitTests.Modules.Fichas.Infrastructure.Security;

public sealed class GeradorTokenConviteTests
{
    private readonly GeradorTokenConvite _gerador = new();

    [Fact]
    public void Gerar_DeveRetornarTokenCompativelComUrlEHashSha256()
    {
        var tokenGerado = _gerador.Gerar();

        Assert.NotEmpty(tokenGerado.TokenOriginal);
        Assert.DoesNotContain("+", tokenGerado.TokenOriginal);
        Assert.DoesNotContain("/", tokenGerado.TokenOriginal);
        Assert.DoesNotContain("=", tokenGerado.TokenOriginal);
        Assert.Equal(64, tokenGerado.TokenHash.Length);
        Assert.Equal(
            _gerador.CalcularHash(tokenGerado.TokenOriginal),
            tokenGerado.TokenHash);
    }

    [Fact]
    public void Gerar_DuasVezes_DeveRetornarTokensDiferentes()
    {
        var primeiroToken = _gerador.Gerar();
        var segundoToken = _gerador.Gerar();

        Assert.NotEqual(
            primeiroToken.TokenOriginal,
            segundoToken.TokenOriginal);
        Assert.NotEqual(
            primeiroToken.TokenHash,
            segundoToken.TokenHash);
    }

    [Fact]
    public void CalcularHash_ParaOMesmoToken_DeveRetornarOMesmoResultado()
    {
        const string tokenOriginal = "token-de-exemplo";

        var primeiroHash = _gerador.CalcularHash(tokenOriginal);
        var segundoHash = _gerador.CalcularHash(tokenOriginal);

        Assert.Equal(primeiroHash, segundoHash);
    }

    [Fact]
    public void CalcularHash_ComTokenVazio_DeveLancarArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => _gerador.CalcularHash("   "));

        Assert.Equal("tokenOriginal", exception.ParamName);
        Assert.Contains(
            "O token original é obrigatório.",
            exception.Message);
    }
}
