using FichaDigital.Api.Modules.Profissionais.Domain;

namespace FichaDigital.UnitTests.Modules.Profissionais.Domain;

public sealed class ProfissionalUsuarioTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveNormalizarDados()
    {
        var profissional = new ProfissionalUsuario(
            "  Lia Silva  ",
            "  lia@example.com  ");

        Assert.NotEqual(Guid.Empty, profissional.Id);
        Assert.Equal("Lia Silva", profissional.NomeCompleto);
        Assert.Equal("lia@example.com", profissional.Email);
        Assert.Equal("lia@example.com", profissional.UserName);
    }

    [Fact]
    public void Criar_ComNomeVazio_DeveLancarArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new ProfissionalUsuario(
                " ",
                "lia@example.com"));

        Assert.Equal("nomeCompleto", exception.ParamName);
    }
}
