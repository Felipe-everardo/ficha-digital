using FichaDigital.Api.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace FichaDigital.UnitTests.Infrastructure;

public sealed class ConfiguracaoPilotoTests
{
    [Fact]
    public void DemoMantemConfiguracaoExistente()
    {
        ConfiguracaoPiloto.Validar(new ConfigurationBuilder().Build(), new Ambiente("Production"));
    }

    [Fact]
    public void PilotoSemConfiguracaoNaoInicia()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ConfiguracaoPiloto.Validar(new ConfigurationBuilder().Build(), new Ambiente("Piloto")));
    }

    [Theory]
    [InlineData("ConnectionStrings:DefaultConnection", "Server=sql-fichadigital-demo.database.windows.net;Database=FichaDigitalEstudioDb;Encrypt=True")]
    [InlineData("ConnectionStrings:DefaultConnection", "Server=sql-estudio.database.windows.net;Database=FichaDigitalDb;Encrypt=True")]
    [InlineData("ConnectionStrings:DefaultConnection", "Server=sql-estudio.database.windows.net;Database=FichaDigitalEstudioDb;Encrypt=False")]
    [InlineData("ConnectionStrings:DefaultConnection", "Server=sql-estudio.database.windows.net;Database=FichaDigitalEstudioDb;Encrypt=True;TrustServerCertificate=True")]
    [InlineData("DataProtection:ApplicationName", "FichaDigital")]
    [InlineData("DataProtection:KeysPath", "chaves-relativas")]
    [InlineData("DatabaseInitialization:ApplyMigrationsOnStartup", "true")]
    [InlineData("DatabaseProvider", "Sqlite")]
    public void PilotoRejeitaConfiguracaoInsegura(string chave, string valor)
    {
        var valores = ConfiguracaoValida();
        valores[chave] = valor;
        Assert.Throws<InvalidOperationException>(() => ConfiguracaoPiloto.Validar(
            new ConfigurationBuilder().AddInMemoryCollection(valores).Build(), new Ambiente("Piloto")));
    }

    [Fact]
    public void PilotoAceitaConfiguracaoSeparada()
    {
        ConfiguracaoPiloto.Validar(new ConfigurationBuilder()
            .AddInMemoryCollection(ConfiguracaoValida()).Build(), new Ambiente("Piloto"));
    }

    private static Dictionary<string, string?> ConfiguracaoValida() => new()
    {
        ["ConnectionStrings:DefaultConnection"] = "Server=sql-estudio.database.windows.net;Database=FichaDigitalEstudioDb;Encrypt=True;TrustServerCertificate=False",
        ["DataProtection:ApplicationName"] = "FichaDigital.Estudio.Piloto",
        ["DataProtection:KeysPath"] = Path.Combine(Path.GetTempPath(), "ficha-piloto-keys"),
        ["DatabaseInitialization:ApplyMigrationsOnStartup"] = "false"
    };

    private sealed class Ambiente(string nome) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = nome;
        public string ApplicationName { get; set; } = "FichaDigital.Api";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
