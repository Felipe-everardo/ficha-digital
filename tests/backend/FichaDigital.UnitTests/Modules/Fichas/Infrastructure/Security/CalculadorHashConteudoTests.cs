using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;

namespace FichaDigital.UnitTests.Modules.Fichas.Infrastructure.Security;

public sealed class CalculadorHashConteudoTests
{
    private readonly CalculadorHashConteudo _calculador = new();

    [Fact]
    public void Calcular_ParaMesmoConteudo_DeveRetornarMesmoHashSha256()
    {
        var primeiroHash = _calculador.Calcular("Conteúdo do termo");
        var segundoHash = _calculador.Calcular("Conteúdo do termo");

        Assert.Equal(64, primeiroHash.Length);
        Assert.Equal(primeiroHash, segundoHash);
    }

    [Fact]
    public void Calcular_ParaConteudosDiferentes_DeveRetornarHashesDiferentes()
    {
        var primeiroHash = _calculador.Calcular("Versão A");
        var segundoHash = _calculador.Calcular("Versão B");

        Assert.NotEqual(primeiroHash, segundoHash);
    }
}
