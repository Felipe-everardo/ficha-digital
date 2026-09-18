using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.UnitTests.Modules.Fichas.Domain;

public sealed class FichaTests
{
    [Fact]
    public void Criar_ComResponsavelEProcedimento_DeveRegistrarContextoDaFicha()
    {
        var clienteId = Guid.NewGuid();
        var profissionalId = Guid.NewGuid();

        var ficha = new Ficha(
            clienteId,
            profissionalId,
            "  Marina Tattoo  ",
            TipoProcedimento.Tatuagem);

        Assert.Equal(clienteId, ficha.ClienteId);
        Assert.Equal(profissionalId, ficha.ProfissionalResponsavelId);
        Assert.Equal("Marina Tattoo", ficha.ProfissionalResponsavelNome);
        Assert.Equal(TipoProcedimento.Tatuagem, ficha.TipoProcedimento);
    }

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
    public void Criar_ComResponsavelSemProcedimento_DeveLancarArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => new Ficha(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Marina Tattoo",
            TipoProcedimento.NaoInformado));

        Assert.Equal("tipoProcedimento", exception.ParamName);
    }

    [Fact]
    public void EnviarConvite_QuandoFichaEstaEmRascunho_DeveAlterarStatus()
    {
        var ficha = new Ficha(Guid.NewGuid());

        ficha.EnviarConvite();

        Assert.Equal(StatusFicha.ConviteEnviado, ficha.Status);
    }

    [Fact]
    public void AlterarStatus_DeveIncrementarVersaoDeConcorrencia()
    {
        var ficha = new Ficha(Guid.NewGuid());

        Assert.Equal(0, ficha.VersaoConcorrencia);

        ficha.EnviarConvite();
        Assert.Equal(1, ficha.VersaoConcorrencia);

        ficha.IniciarPreenchimento();
        Assert.Equal(2, ficha.VersaoConcorrencia);
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
    public void ConcluirAnamnese_QuandoFichaEstaEmPreenchimento_DeveAlterarStatus()
    {
        var ficha = CriarFichaEmPreenchimento();

        ficha.ConcluirAnamnese();

        Assert.Equal(StatusFicha.AnamnesePreenchida, ficha.Status);
    }

    [Fact]
    public void ConcluirAnamnese_QuandoFichaEstaEmRascunho_DeveLancarInvalidOperationException()
    {
        var ficha = new Ficha(Guid.NewGuid());

        var exception = Assert.Throws<InvalidOperationException>(
            ficha.ConcluirAnamnese);

        Assert.Equal(
            "Somente uma ficha em preenchimento pode concluir a anamnese.",
            exception.Message);
        Assert.Equal(StatusFicha.Rascunho, ficha.Status);
    }

    [Fact]
    public void AutorizarProcedimento_QuandoAnamneseFoiPreenchida_DeveAlterarStatus()
    {
        var ficha = CriarFichaComAnamnesePreenchida();

        ficha.AutorizarProcedimento();

        Assert.Equal(StatusFicha.AutorizadaParaProcedimento, ficha.Status);
    }

    [Fact]
    public void AutorizarProcedimento_QuandoAnamneseEstaEmPreenchimento_DeveLancarInvalidOperationException()
    {
        var ficha = CriarFichaEmPreenchimento();

        var exception = Assert.Throws<InvalidOperationException>(
            ficha.AutorizarProcedimento);

        Assert.Equal(
            "Somente uma ficha com anamnese preenchida pode autorizar o procedimento.",
            exception.Message);
        Assert.Equal(StatusFicha.EmPreenchimento, ficha.Status);
    }

    [Fact]
    public void ConfirmarRevisao_QuandoFichaEstaAutorizada_DeveAlterarStatus()
    {
        var ficha = CriarFichaAutorizada();

        ficha.ConfirmarRevisaoProfissional();

        Assert.Equal(StatusFicha.RevisadaPeloProfissional, ficha.Status);
    }

    [Fact]
    public void ConfirmarRevisao_QuandoAguardaAceiteDoCliente_DeveFalhar()
    {
        var ficha = CriarFichaComAnamnesePreenchida();

        var exception = Assert.Throws<InvalidOperationException>(
            ficha.ConfirmarRevisaoProfissional);

        Assert.Equal(
            "Somente uma ficha autorizada pode ser revisada pelo profissional.",
            exception.Message);
        Assert.Equal(StatusFicha.AnamnesePreenchida, ficha.Status);
    }

    [Fact]
    public void ConcluirProcedimento_QuandoFichaFoiRevisada_DeveAlterarStatus()
    {
        var ficha = CriarFichaRevisada();

        ficha.ConcluirProcedimento();

        Assert.Equal(StatusFicha.Concluida, ficha.Status);
    }

    [Fact]
    public void ConcluirProcedimento_QuandoFichaEstaSomenteAutorizada_DeveLancarInvalidOperationException()
    {
        var ficha = CriarFichaAutorizada();

        var exception = Assert.Throws<InvalidOperationException>(
            ficha.ConcluirProcedimento);

        Assert.Equal(
            "Somente uma ficha revisada pelo profissional pode ser concluída.",
            exception.Message);
        Assert.Equal(StatusFicha.AutorizadaParaProcedimento, ficha.Status);
    }

    [Fact]
    public void ConcluirFluxoLegado_QuandoFichaEstaEmPreenchimento_DeveAlterarStatus()
    {
        var ficha = CriarFichaEmPreenchimento();

        ficha.ConcluirFluxoLegado();

        Assert.Equal(StatusFicha.Concluida, ficha.Status);
    }

    [Fact]
    public void ConcluirFluxoLegado_QuandoFichaEstaEmRascunho_DeveLancarInvalidOperationException()
    {
        var ficha = new Ficha(Guid.NewGuid());

        var exception = Assert.Throws<InvalidOperationException>(
            ficha.ConcluirFluxoLegado);

        Assert.Equal(
            "Somente uma ficha em preenchimento pode concluir o fluxo legado.",
            exception.Message);
        Assert.Equal(StatusFicha.Rascunho, ficha.Status);
    }

    private static Ficha CriarFichaEmPreenchimento()
    {
        var ficha = new Ficha(Guid.NewGuid());
        ficha.EnviarConvite();
        ficha.IniciarPreenchimento();
        return ficha;
    }

    private static Ficha CriarFichaComAnamnesePreenchida()
    {
        var ficha = CriarFichaEmPreenchimento();
        ficha.ConcluirAnamnese();
        return ficha;
    }

    private static Ficha CriarFichaAutorizada()
    {
        var ficha = CriarFichaComAnamnesePreenchida();
        ficha.AutorizarProcedimento();
        return ficha;
    }

    private static Ficha CriarFichaRevisada()
    {
        var ficha = CriarFichaAutorizada();
        ficha.ConfirmarRevisaoProfissional();
        return ficha;
    }
}
