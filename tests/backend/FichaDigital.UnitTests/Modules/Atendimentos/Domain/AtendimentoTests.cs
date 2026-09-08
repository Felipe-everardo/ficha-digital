using FichaDigital.Api.Modules.Atendimentos.Domain;

namespace FichaDigital.UnitTests.Modules.Atendimentos.Domain;

public sealed class AtendimentoTests
{
    [Fact]
    public void Criar_ComValoresValidos_DeveCalcularValorFinal()
    {
        var agora = DateTimeOffset.UtcNow;

        var atendimento = new Atendimento(
            Guid.NewGuid(),
            new DateOnly(2026, 9, 7),
            300m,
            25.50m,
            FormaPagamento.Pix,
            agora);

        Assert.Equal(274.50m, atendimento.ValorFinal);
        Assert.Equal(agora, atendimento.RegistradoEmUtc);
        Assert.Equal(agora, atendimento.AtualizadoEmUtc);
    }

    [Fact]
    public void Atualizar_ComDescontoMaiorQueValor_DeveFalhar()
    {
        var atendimento = new Atendimento(
            Guid.NewGuid(),
            new DateOnly(2026, 9, 7),
            100m,
            0m,
            FormaPagamento.Dinheiro,
            DateTimeOffset.UtcNow);

        var excecao = Assert.Throws<ArgumentException>(() =>
            atendimento.Atualizar(
                new DateOnly(2026, 9, 7),
                100m,
                120m,
                FormaPagamento.Dinheiro,
                DateTimeOffset.UtcNow));

        Assert.Equal("desconto", excecao.ParamName);
    }

    [Fact]
    public void Criar_ComMaisDeDuasCasasDecimais_DeveFalhar()
    {
        var excecao = Assert.Throws<ArgumentException>(() =>
            new Atendimento(
                Guid.NewGuid(),
                new DateOnly(2026, 9, 7),
                100.999m,
                0m,
                FormaPagamento.Pix,
                DateTimeOffset.UtcNow));

        Assert.Equal("valorCobrado", excecao.ParamName);
    }
}
