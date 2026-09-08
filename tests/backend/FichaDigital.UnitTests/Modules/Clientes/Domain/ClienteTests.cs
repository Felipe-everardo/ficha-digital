using FichaDigital.Api.Modules.Clientes.Domain;

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
        Assert.Null(cliente.Celular);
        Assert.False(cliente.DadosPessoaisPreenchidos);
    }

    [Fact]
    public void PreencherDadosPessoais_ComDadosValidos_DeveNormalizarEConcluir()
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

        cliente.PreencherDadosPessoais(
            "  Ana Silva  ",
            "  Aninha  ",
            "  ela/dela  ",
            new DateOnly(1995, 6, 15),
            "  (21) 99999-9999  ",
            "  ana@example.com  ",
            "  @ana  ",
            "  Maria Silva  ",
            "  (21) 98888-8888  ",
            preenchidosEmUtc);

        Assert.Equal("Ana Silva", cliente.NomeCompleto);
        Assert.Equal("Aninha", cliente.NomeParaExibicao);
        Assert.Equal("@ana", cliente.Instagram);
        Assert.Equal("Maria Silva", cliente.ContatoEmergenciaNome);
        Assert.Equal("(21) 98888-8888", cliente.ContatoEmergenciaCelular);
        Assert.Equal(preenchidosEmUtc, cliente.DadosPessoaisPreenchidosEmUtc);
        Assert.True(cliente.DadosPessoaisPreenchidos);
    }

    [Fact]
    public void PreencherDadosPessoais_ComContatoEmergenciaIncompleto_DeveLancarArgumentException()
    {
        var cliente = new Cliente("Ana");

        var exception = Assert.Throws<ArgumentException>(() =>
            cliente.PreencherDadosPessoais(
                "Ana Silva",
                null,
                null,
                new DateOnly(1995, 6, 15),
                "(21) 99999-9999",
                null,
                null,
                "Maria Silva",
                null,
                DateTimeOffset.UtcNow));

        Assert.Equal("contatoEmergenciaCelular", exception.ParamName);
    }

    [Fact]
    public void Criar_ComDadosValidos_DeveNormalizarEArmazenarOsDados()
    {
        // Arrange
        var dataNascimento = new DateOnly(1995, 6, 15);

        // Act
        var cliente = new Cliente(
            "  Ana Silva  ",
            "  Ana  ",
            "  ela/dela  ",
            dataNascimento,
            "  (21) 99999-9999  ",
            "  ana@example.com  ");

        // Assert
        Assert.NotEqual(Guid.Empty, cliente.Id);
        Assert.Equal("Ana Silva", cliente.NomeCompleto);
        Assert.Equal("Ana", cliente.NomeSocial);
        Assert.Equal("ela/dela", cliente.Pronomes);
        Assert.Equal(dataNascimento, cliente.DataNascimento);
        Assert.Equal("(21) 99999-9999", cliente.Celular);
        Assert.Equal("ana@example.com", cliente.Email);
    }

    [Fact]
    public void Criar_ComCelularVazio_DeveLancarArgumentException()
    {
        // Arrange
        var dataNascimento = new DateOnly(1995, 6, 15);
        var celularVazio = " ";


        // Act e assert
        var exception = Assert.Throws<ArgumentException>(() => new Cliente(
            "  Ana Silva  ",
            "  Ana  ",
            "  ela/dela  ",
            dataNascimento,
            celularVazio,
            "  ana@example.com  "));

        Assert.Equal("celular", exception.ParamName);
        Assert.Contains("O celular é obrigatório.", exception.Message);
    }

    [Fact]
    public void Criar_ComCamposOpcionaisEmBranco_DeveArmazenarNull()
    {
        
        var dataNascimento = new DateOnly(1995, 6, 15);

        
        var cliente = new Cliente(
            "  Ana Silva  ",
            "    ",
            "    ",
            dataNascimento,
            "  (21) 99999-9999  ",
            "   ");

        Assert.Null(cliente.NomeSocial);
        Assert.Null(cliente.Pronomes);
        Assert.Null(cliente.Email);
    }

    [Fact]
    public void Criar_ComDataNascimentoFutura_DeveLancarArgumentException()
    {
        var dataFutura = DateOnly
            .FromDateTime(DateTime.UtcNow)
            .AddYears(1);

        var exception = Assert.Throws<ArgumentException>(() => new Cliente(
           "  Ana Silva  ",
           "  Ana  ",
           "  ela/dela  ",
           dataFutura,
           "  (21) 99999-9999  ",
           "  ana@example.com  "));

        Assert.Equal("dataNascimento", exception.ParamName);
    }

    [Fact]
    public void AtualizarContato_ComDadosValidos_DeveNormalizarEAtualizarContato()
    {
        var dataNascimento = new DateOnly(1995, 6, 15);

        var cliente = new Cliente(
            "  Ana Silva  ",
            "    ",
            "    ",
            dataNascimento,
            "  (21) 99999-9999  ",
            "   ");

        string novoCelular = " (21) 222222222 ";
        string novoEmail = " teste@teste.com ";

        cliente.AtualizarContato(novoCelular, novoEmail);
            
        Assert.Equal("(21) 222222222", cliente.Celular);
        Assert.Equal("teste@teste.com", cliente.Email);

    }

    [Fact]
    public void AtualizarDadosPessoais_ComNovoContato_DeveManterCadastroAtualizado()
    {
        var cliente = new Cliente(
            "Ana Silva",
            null,
            "ela/dela",
            new DateOnly(1995, 6, 15),
            "(21) 99999-9999",
            "ana@example.com");
        var atualizadoEmUtc = DateTimeOffset.UtcNow.AddMinutes(1);

        cliente.AtualizarDadosPessoais(
            "Ana Silva",
            null,
            "ela/dela",
            new DateOnly(1995, 6, 15),
            "(21) 97777-7777",
            "novo-email@example.com",
            "@ana",
            null,
            null,
            atualizadoEmUtc);

        Assert.Equal("(21) 97777-7777", cliente.Celular);
        Assert.Equal("novo-email@example.com", cliente.Email);
        Assert.Equal("@ana", cliente.Instagram);
        Assert.Equal(atualizadoEmUtc, cliente.DadosPessoaisPreenchidosEmUtc);
    }
}
