using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.UnitTests.Modules.Fichas.Domain;

public sealed class AceiteTermoConsentimentoTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveNormalizarEArmazenarEvidencias()
    {
        var fichaId = Guid.NewGuid();
        var hash = new string('a', 64);
        var aceitoEmUtc = DateTimeOffset.UtcNow;

        var aceite = new AceiteTermoConsentimento(
            fichaId,
            versaoTermo: 1,
            conteudoTermo: "  Conteúdo exibido  ",
            conteudoHash: hash.ToUpperInvariant(),
            nomeAssinante: "  Ana Silva  ",
            aceitoEmUtc);

        Assert.NotEqual(Guid.Empty, aceite.Id);
        Assert.Equal(fichaId, aceite.FichaId);
        Assert.Equal(1, aceite.VersaoTermo);
        Assert.Equal("Conteúdo exibido", aceite.ConteudoTermo);
        Assert.Equal(hash, aceite.ConteudoHash);
        Assert.Equal("Ana Silva", aceite.NomeAssinante);
        Assert.Equal(aceitoEmUtc, aceite.AceitoEmUtc);
    }

    [Fact]
    public void Criar_ComHashInvalido_DeveLancarArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new AceiteTermoConsentimento(
                Guid.NewGuid(),
                versaoTermo: 1,
                conteudoTermo: "Conteúdo exibido",
                conteudoHash: "hash-curto",
                nomeAssinante: "Ana Silva",
                DateTimeOffset.UtcNow));

        Assert.Equal("conteudoHash", exception.ParamName);
    }
}
