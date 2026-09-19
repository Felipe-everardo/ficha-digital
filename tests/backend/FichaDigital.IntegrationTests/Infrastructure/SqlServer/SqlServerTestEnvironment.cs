namespace FichaDigital.IntegrationTests.Infrastructure.SqlServer;

public static class SqlServerTestEnvironment
{
    public const string SkipReason =
        "Defina RUN_SQLSERVER_TESTS=true e disponibilize Docker para executar.";

    public static bool Enabled => string.Equals(
        Environment.GetEnvironmentVariable("RUN_SQLSERVER_TESTS"),
        "true",
        StringComparison.OrdinalIgnoreCase);
}
