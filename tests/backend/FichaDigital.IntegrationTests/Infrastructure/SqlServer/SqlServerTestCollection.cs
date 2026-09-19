namespace FichaDigital.IntegrationTests.Infrastructure.SqlServer;

[CollectionDefinition(Name)]
public sealed class SqlServerTestCollection :
    ICollectionFixture<SqlServerContainerFixture>
{
    public const string Name = "SQL Server real";
}
