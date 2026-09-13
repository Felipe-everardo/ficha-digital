using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Shared.Domain;

namespace FichaDigital.UnitTests.Modules.Fichas.Domain;

public sealed class DadosPessoaisFichaTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveGuardarRetratoNormalizado()
    {
        var fichaId = Guid.NewGuid();
        var confirmadoEmUtc = DateTimeOffset.UtcNow;

        var dados = new DadosPessoaisFicha(
            fichaId,
            CriarDados(),
            confirmadoEmUtc);

        Assert.Equal(fichaId, dados.FichaId);
        Assert.Equal("Ana Silva", dados.NomeCompleto);
        Assert.Equal("Ana", dados.NomeParaExibicao);
        Assert.Equal("52998224725", dados.Cpf);
        Assert.Equal("20040002", dados.Cep);
        Assert.Equal("RJ", dados.Estado);
        Assert.Equal(confirmadoEmUtc, dados.ConfirmadosEmUtc);
    }

    [Fact]
    public void Atualizar_DeveManterVinculoComAFicha()
    {
        var fichaId = Guid.NewGuid();
        var dados = new DadosPessoaisFicha(
            fichaId,
            CriarDados(),
            DateTimeOffset.UtcNow);

        dados.Atualizar(
            CriarDados(
                nomeCompleto: "Ana Silva Atualizada",
                nomeSocial: null,
                email: "novo@example.com"),
            DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.Equal(fichaId, dados.FichaId);
        Assert.Equal("Ana Silva Atualizada", dados.NomeParaExibicao);
        Assert.Equal("novo@example.com", dados.Email);
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("529.982.247-24")]
    [InlineData("123")]
    public void CriarDados_ComCpfInvalido_DeveRejeitar(string cpf)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CriarDados(cpf: cpf));

        Assert.Equal("cpf", exception.ParamName);
    }

    [Fact]
    public void CriarDados_ComContatoEmergenciaIncompleto_DeveRejeitar()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CriarDados(contatoEmergenciaNome: "Maria"));

        Assert.Equal("contatoEmergenciaCelular", exception.ParamName);
    }

    private static DadosPessoaisInformados CriarDados(
        string nomeCompleto = "  Ana Silva  ",
        string? nomeSocial = "  Ana  ",
        string? email = "  ana@example.com  ",
        string cpf = "529.982.247-25",
        string? contatoEmergenciaNome = null,
        string? contatoEmergenciaCelular = null)
    {
        return new DadosPessoaisInformados(
            nomeCompleto,
            nomeSocial,
            pronomes: "  ela/dela  ",
            estadoCivil: "  Solteira  ",
            dataNascimento: new DateOnly(1995, 6, 15),
            cpf,
            celular: "  (21) 99999-9999  ",
            telefoneAdicional: null,
            email,
            instagram: null,
            contatoEmergenciaNome,
            contatoEmergenciaCelular,
            cep: "20040-002",
            logradouro: "  Rua da Assembleia  ",
            numero: "  10  ",
            complemento: null,
            bairro: "  Centro  ",
            cidade: "  Rio de Janeiro  ",
            estado: "rj");
    }
}
