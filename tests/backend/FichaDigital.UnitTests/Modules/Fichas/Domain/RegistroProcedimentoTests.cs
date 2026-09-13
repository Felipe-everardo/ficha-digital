using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.UnitTests.Modules.Fichas.Domain;

public sealed class RegistroProcedimentoTests
{
    private const string AssinaturaPng =
        "data:image/png;base64," +
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0l" +
        "EQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";

    [Fact]
    public void CriarRevisao_SemConferenciaDosDados_DeveRejeitar()
    {
        Assert.Throws<ArgumentException>(() => new RevisaoProfissional(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Profissional",
            dadosDaFichaConferidos: false,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CriarRegistroTatuagem_ComSinalMaiorQueTotal_DeveRejeitar()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RegistroTatuagem(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Profissional",
                "Arte",
                "Material",
                "Braço",
                null,
                valorTotal: 100m,
                valorSinal: 101m,
                FormaPagamento.Pix,
                "Profissional",
                AssinaturaPng,
                "{}",
                new string('a', 64),
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CriarRegistroPiercing_ComDadosValidos_DeveCalcularRestante()
    {
        var registro = new RegistroPiercing(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Profissional",
            "Titânio",
            "14G",
            "Hélix",
            null,
            valorTotal: 180m,
            valorSinal: 50m,
            FormaPagamento.Cartao,
            "Profissional",
            AssinaturaPng,
            "{}",
            new string('a', 64),
            DateTimeOffset.UtcNow);

        Assert.Equal(130m, registro.ValorRestante);
    }
}
