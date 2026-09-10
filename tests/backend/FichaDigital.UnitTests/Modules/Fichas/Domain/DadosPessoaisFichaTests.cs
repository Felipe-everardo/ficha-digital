using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.UnitTests.Modules.Fichas.Domain;

public sealed class DadosPessoaisFichaTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveNormalizarORetrato()
    {
        var fichaId = Guid.NewGuid();
        var confirmadoEmUtc = DateTimeOffset.UtcNow;

        var dados = new DadosPessoaisFicha(
            fichaId,
            "  Ana Silva  ",
            "  Ana  ",
            "  ela/dela  ",
            new DateOnly(1995, 6, 15),
            "  (21) 99999-9999  ",
            "  ana@example.com  ",
            null,
            "  Maria  ",
            "  (21) 98888-8888  ",
            confirmadoEmUtc);

        Assert.Equal(fichaId, dados.FichaId);
        Assert.Equal("Ana Silva", dados.NomeCompleto);
        Assert.Equal("Ana", dados.NomeSocial);
        Assert.Equal("Ana", dados.NomeParaExibicao);
        Assert.Equal("ana@example.com", dados.Email);
        Assert.Equal(confirmadoEmUtc, dados.ConfirmadosEmUtc);
    }

    [Fact]
    public void Atualizar_DeveManterVinculoComAFicha()
    {
        var fichaId = Guid.NewGuid();
        var dados = CriarDados(fichaId);

        dados.Atualizar(
            "Ana Silva Atualizada",
            null,
            null,
            new DateOnly(1995, 6, 15),
            "(21) 97777-7777",
            "novo@example.com",
            null,
            null,
            null,
            DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.Equal(fichaId, dados.FichaId);
        Assert.Equal("Ana Silva Atualizada", dados.NomeParaExibicao);
        Assert.Equal("novo@example.com", dados.Email);
    }

    [Fact]
    public void Criar_ComContatoEmergenciaIncompleto_DeveRejeitar()
    {
        Assert.Throws<ArgumentException>(() => new DadosPessoaisFicha(
            Guid.NewGuid(),
            "Ana Silva",
            null,
            null,
            new DateOnly(1995, 6, 15),
            "(21) 99999-9999",
            null,
            null,
            "Maria",
            null,
            DateTimeOffset.UtcNow));
    }

    private static DadosPessoaisFicha CriarDados(Guid fichaId)
    {
        return new DadosPessoaisFicha(
            fichaId,
            "Ana Silva",
            "Ana",
            "ela/dela",
            new DateOnly(1995, 6, 15),
            "(21) 99999-9999",
            "ana@example.com",
            null,
            null,
            null,
            DateTimeOffset.UtcNow);
    }
}
