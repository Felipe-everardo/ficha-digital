using FichaDigital.Api.Modules.Clientes.Domain;

namespace FichaDigital.UnitTests.Modules.Clientes.Domain;

public sealed class ClienteTests
{
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
}
