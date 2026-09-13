using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.UnitTests.Modules.Fichas.Domain;

public sealed class AssinaturaDesenhadaTests
{
    private const string AssinaturaPng =
        "data:image/png;base64," +
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0l" +
        "EQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";

    [Fact]
    public void ValidarENormalizar_ComPng_DeveRetornarConteudo()
    {
        var resultado = AssinaturaDesenhada.ValidarENormalizar(
            $"  {AssinaturaPng}  ",
            "assinatura");

        Assert.Equal(AssinaturaPng, resultado);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("data:image/jpeg;base64,abc")]
    [InlineData("data:image/png;base64,abc")]
    public void ValidarENormalizar_ComConteudoInvalido_DeveRejeitar(
        string? conteudo)
    {
        Assert.Throws<ArgumentException>(() =>
            AssinaturaDesenhada.ValidarENormalizar(conteudo, "assinatura"));
    }
}
