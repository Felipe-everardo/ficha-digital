using FichaDigital.Api.Modules.Fichas.Application;

namespace FichaDigital.UnitTests.Modules.Fichas.Application;

public sealed class HorarioEstudioTests
{
    [Fact]
    public void ObterInicioUtc_DeveConsiderarFusoHorarioDoEstudio()
    {
        var inicioUtc = HorarioEstudio.ObterInicioUtc(
            new DateOnly(2026, 9, 11));

        Assert.Equal(
            new DateTimeOffset(2026, 9, 11, 3, 0, 0, TimeSpan.Zero),
            inicioUtc);
    }

    [Fact]
    public void ObterDataLocal_AntesDaMeiaNoiteUtc_DeveRetornarDiaAnterior()
    {
        var dataLocal = HorarioEstudio.ObterDataLocal(
            new DateTimeOffset(2026, 9, 11, 2, 59, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 9, 10), dataLocal);
    }
}
