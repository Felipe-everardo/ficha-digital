using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Shared.Domain;

namespace FichaDigital.UnitTests.Modules.Clientes.Domain;

public sealed class ClienteTests
{
    [Fact]
    public void CriarApenasComNomeReferencia_DeveIniciarComDadosPessoaisPendentes()
    {
        var cliente = new Cliente("  Ana  ");

        Assert.Equal("Ana", cliente.NomeReferencia);
        Assert.Equal("Ana", cliente.NomeParaExibicao);
        Assert.Null(cliente.NomeCompleto);
        Assert.Null(cliente.Cpf);
        Assert.False(cliente.DadosPessoaisPreenchidos);
    }

    [Fact]
    public void PreencherDadosPessoais_ComDadosValidos_DeveCopiarDadosNormalizados()
    {
        var cliente = new Cliente("Ana");
        var preenchidosEmUtc = new DateTimeOffset(
            2026,
            8,
            20,
            12,
            0,
            0,
            TimeSpan.Zero);
        var dados = CriarDados(
            nomeCompleto: "  Ana Silva  ",
            nomeSocial: "  Aninha  ",
            instagram: "  @ana  ",
            contatoEmergenciaNome: "  Maria Silva  ",
            contatoEmergenciaCelular: "  (21) 98888-8888  ");

        cliente.PreencherDadosPessoais(dados, preenchidosEmUtc);

        Assert.Equal("Ana Silva", cliente.NomeCompleto);
        Assert.Equal("Aninha", cliente.NomeParaExibicao);
        Assert.Equal("52998224725", cliente.Cpf);
        Assert.Equal("@ana", cliente.Instagram);
        Assert.Equal("Maria Silva", cliente.ContatoEmergenciaNome);
        Assert.Equal("20040002", cliente.Cep);
        Assert.Equal("RJ", cliente.Estado);
        Assert.Equal(preenchidosEmUtc, cliente.DadosPessoaisPreenchidosEmUtc);
        Assert.True(cliente.DadosPessoaisPreenchidos);
    }

    [Fact]
    public void PreencherDadosPessoais_DuasVezes_DeveRejeitar()
    {
        var cliente = new Cliente("Ana");
        cliente.PreencherDadosPessoais(
            CriarDados(),
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            cliente.PreencherDadosPessoais(
                CriarDados(),
                DateTimeOffset.UtcNow));

        Assert.Equal(
            "Os dados pessoais do cliente já foram preenchidos.",
            exception.Message);
    }

    [Fact]
    public void AtualizarDadosPessoais_ComNovoContato_DeveManterCadastroAtualizado()
    {
        var cliente = new Cliente(CriarDados());
        var atualizadoEmUtc = DateTimeOffset.UtcNow.AddMinutes(1);

        cliente.AtualizarDadosPessoais(
            CriarDados(
                celular: "(21) 97777-7777",
                email: "novo-email@example.com",
                instagram: "@ana_nova"),
            atualizadoEmUtc);

        Assert.Equal("(21) 97777-7777", cliente.Celular);
        Assert.Equal("novo-email@example.com", cliente.Email);
        Assert.Equal("@ana_nova", cliente.Instagram);
        Assert.Equal(atualizadoEmUtc, cliente.DadosPessoaisPreenchidosEmUtc);
    }

    [Fact]
    public void AtualizarDadosPessoais_QuandoCadastroEstaPendente_DeveRejeitar()
    {
        var cliente = new Cliente("Ana");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            cliente.AtualizarDadosPessoais(
                CriarDados(),
                DateTimeOffset.UtcNow));

        Assert.Equal(
            "Os dados pessoais do cliente ainda não foram preenchidos.",
            exception.Message);
    }

    [Fact]
    public void AtualizarContato_ComDadosValidos_DeveNormalizarEAtualizarContato()
    {
        var cliente = new Cliente(CriarDados());

        cliente.AtualizarContato(
            " (21) 22222-2222 ",
            " teste@teste.com ");

        Assert.Equal("(21) 22222-2222", cliente.Celular);
        Assert.Equal("teste@teste.com", cliente.Email);
    }

    private static DadosPessoaisInformados CriarDados(
        string nomeCompleto = "Ana Silva",
        string? nomeSocial = "Ana",
        string celular = "(21) 99999-9999",
        string? email = "ana@example.com",
        string? instagram = null,
        string? contatoEmergenciaNome = null,
        string? contatoEmergenciaCelular = null)
    {
        return new DadosPessoaisInformados(
            nomeCompleto,
            nomeSocial,
            pronomes: "ela/dela",
            estadoCivil: "Solteira",
            dataNascimento: new DateOnly(1995, 6, 15),
            cpf: "529.982.247-25",
            celular,
            telefoneAdicional: null,
            email,
            instagram,
            contatoEmergenciaNome,
            contatoEmergenciaCelular,
            cep: "20040-002",
            logradouro: "Rua da Assembleia",
            numero: "10",
            complemento: null,
            bairro: "Centro",
            cidade: "Rio de Janeiro",
            estado: "rj");
    }
}
