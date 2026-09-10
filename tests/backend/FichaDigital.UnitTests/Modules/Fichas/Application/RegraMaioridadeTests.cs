using FichaDigital.Api.Modules.Fichas.Application;

namespace FichaDigital.UnitTests.Modules.Fichas.Application;

public sealed class RegraMaioridadeTests
{
    private static readonly DateTimeOffset InstanteNoRio =
        new(2026, 9, 8, 15, 0, 0, TimeSpan.FromHours(-3));

    [Fact]
    public void EhMaiorDeIdade_NoDiaEmQueCompletaDezoitoAnos_DeveRetornarTrue()
    {
        var resultado = RegraMaioridade.EhMaiorDeIdade(
            new DateOnly(2008, 9, 8),
            InstanteNoRio);

        Assert.True(resultado);
    }

    [Fact]
    public void EhMaiorDeIdade_UmDiaAntesDeCompletarDezoitoAnos_DeveRetornarFalse()
    {
        var resultado = RegraMaioridade.EhMaiorDeIdade(
            new DateOnly(2008, 9, 9),
            InstanteNoRio);

        Assert.False(resultado);
    }
}
