using FichaDigital.Api.Modules.Financeiro.Domain;

namespace FichaDigital.UnitTests.Modules.Financeiro.Domain;

public sealed class DespesaTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveNormalizarEDefinirAuditoria()
    {
        var profissionalId = Guid.NewGuid();
        var agora = DateTimeOffset.UtcNow;

        var despesa = new Despesa(
            new DateOnly(2026, 9, 7),
            CategoriaDespesa.Materiais,
            "  Luvas descartáveis  ",
            89.90m,
            profissionalId,
            "  Profissional Teste  ",
            agora);

        Assert.Equal("Luvas descartáveis", despesa.Descricao);
        Assert.Equal("Profissional Teste", despesa.ProfissionalNome);
        Assert.Equal(89.90m, despesa.Valor);
        Assert.Equal(profissionalId, despesa.ProfissionalId);
        Assert.Equal(agora, despesa.RegistradaEmUtc);
        Assert.Equal(agora, despesa.AtualizadaEmUtc);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Criar_ComValorNaoPositivo_DeveFalhar(decimal valor)
    {
        var excecao = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Despesa(
                new DateOnly(2026, 9, 7),
                CategoriaDespesa.Contas,
                "Energia",
                valor,
                Guid.NewGuid(),
                "Profissional Teste",
                DateTimeOffset.UtcNow));

        Assert.Equal("valor", excecao.ParamName);
    }

    [Fact]
    public void Atualizar_ComMaisDeDuasCasasDecimais_DeveFalhar()
    {
        var despesa = new Despesa(
            new DateOnly(2026, 9, 7),
            CategoriaDespesa.Contas,
            "Energia",
            100m,
            Guid.NewGuid(),
            "Profissional Teste",
            DateTimeOffset.UtcNow);

        var excecao = Assert.Throws<ArgumentException>(() =>
            despesa.Atualizar(
                new DateOnly(2026, 9, 7),
                CategoriaDespesa.Contas,
                "Energia",
                100.999m,
                DateTimeOffset.UtcNow));

        Assert.Equal("valor", excecao.ParamName);
    }
}
