using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.UnitTests.Modules.Fichas.Domain;

public sealed class FichaTests
{
    [Fact]
    public void Criar_ComClienteValido_DeveIniciarComoRascunho()
    {
        // Arrange
        var clienteId = Guid.NewGuid();

        // Act
        var ficha = new Ficha(clienteId);

        // Assert
        Assert.NotEqual(Guid.Empty, ficha.Id);
        Assert.Equal(clienteId, ficha.ClienteId);
        Assert.Equal(StatusFicha.Rascunho, ficha.Status);
        Assert.NotEqual(default, ficha.CriadaEmUtc);
    }

    [Fact]
    public void Criar_ComClienteVazio_DeveLancarArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new Ficha(Guid.Empty));

        Assert.Equal("clienteId", exception.ParamName);
        Assert.Contains("O cliente é obrigatório.", exception.Message);
    }

    [Fact]
    public void EnviarConvite_QuandoFichaEstaEmRascunho_DeveAlterarStatus()
    {
        var ficha = new Ficha(Guid.NewGuid());

        ficha.EnviarConvite();

        Assert.Equal(StatusFicha.ConviteEnviado, ficha.Status);
    }

    [Fact]
    public void EnviarConvite_QuandoConviteJaFoiEnviado_DeveLancarInvalidOperationException()
    {
        var ficha = new Ficha(Guid.NewGuid());
        ficha.EnviarConvite();

        var exception = Assert.Throws<InvalidOperationException>(
            ficha.EnviarConvite);

        Assert.Equal(
            "Somente uma ficha em rascunho pode ter o convite enviado.",
            exception.Message);
        Assert.Equal(StatusFicha.ConviteEnviado, ficha.Status);
    }

    [Fact]
    public void IniciarPreenchimento_QuandoConviteFoiEnviado_DeveAlterarStatus()
    {
        var ficha = new Ficha(Guid.NewGuid());
        ficha.EnviarConvite();

        ficha.IniciarPreenchimento();

        Assert.Equal(StatusFicha.EmPreenchimento, ficha.Status);
    }

    [Fact]
    public void IniciarPreenchimento_QuandoFichaEstaEmRascunho_DeveLancarInvalidOperationException()
    {
        var ficha = new Ficha(Guid.NewGuid());

        var exception = Assert.Throws<InvalidOperationException>(
            ficha.IniciarPreenchimento);

        Assert.Equal(
            "Somente uma ficha com convite enviado pode iniciar o preenchimento.",
            exception.Message);
        Assert.Equal(StatusFicha.Rascunho, ficha.Status);
    }

    [Fact]
    public void Concluir_QuandoFichaEstaEmPreenchimento_DeveAlterarStatus()
    {
        var ficha = new Ficha(Guid.NewGuid());
        ficha.EnviarConvite();
        ficha.IniciarPreenchimento();

        ficha.Concluir();

        Assert.Equal(StatusFicha.Concluida, ficha.Status);
    }

    [Fact]
    public void Concluir_QuandoFichaEstaEmRascunho_DeveLancarInvalidOperationException()
    {
        var ficha = new Ficha(Guid.NewGuid());

        var exception = Assert.Throws<InvalidOperationException>(
            ficha.Concluir);

        Assert.Equal(
            "Somente uma ficha em preenchimento pode ser concluída.",
            exception.Message);
        Assert.Equal(StatusFicha.Rascunho, ficha.Status);
    }
}
