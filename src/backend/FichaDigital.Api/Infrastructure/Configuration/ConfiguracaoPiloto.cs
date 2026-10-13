using Microsoft.Data.SqlClient;

namespace FichaDigital.Api.Infrastructure.Configuration;

public static class ConfiguracaoPiloto
{
    public static void Validar(IConfiguration configuration, IHostEnvironment environment)
    {
        if (!environment.IsEnvironment("Piloto")) return;

        var conexao = new SqlConnectionStringBuilder(
            configuration.GetConnectionString("DefaultConnection"));
        if (string.IsNullOrWhiteSpace(conexao.DataSource) ||
            string.IsNullOrWhiteSpace(conexao.InitialCatalog) ||
            conexao.InitialCatalog.Equals("FichaDigitalDb", StringComparison.OrdinalIgnoreCase) ||
            conexao.DataSource.Contains("localdb", StringComparison.OrdinalIgnoreCase) ||
            conexao.DataSource.Contains("sql-fichadigital-demo", StringComparison.OrdinalIgnoreCase) ||
            conexao.TrustServerCertificate ||
            conexao.Encrypt == SqlConnectionEncryptOption.Optional ||
            string.Equals(configuration["DatabaseProvider"], "Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("O piloto exige conexão SQL Server exclusiva, criptografada e com certificado validado.");
        }

        var nomeAplicacao = configuration["DataProtection:ApplicationName"];
        var caminhoChaves = configuration["DataProtection:KeysPath"];
        if (string.IsNullOrWhiteSpace(nomeAplicacao) || nomeAplicacao == "FichaDigital" ||
            string.IsNullOrWhiteSpace(caminhoChaves) || !Path.IsPathFullyQualified(caminhoChaves))
        {
            throw new InvalidOperationException("Configure nome exclusivo e diretório persistente absoluto para as chaves do piloto.");
        }

        if (configuration.GetValue<bool>("DatabaseInitialization:ApplyMigrationsOnStartup"))
        {
            throw new InvalidOperationException("As migrations do piloto devem ser aplicadas separadamente da aplicação.");
        }
    }
}
