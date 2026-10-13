using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Application;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Profissionais.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.RegularExpressions;

namespace FichaDigital.IntegrationTests.Infrastructure.SqlServer;

[Collection(SqlServerTestCollection.Name)]
public sealed class SqlServerCompatibilityTests(
    SqlServerContainerFixture fixture)
{
    [Fact(
        Skip = SqlServerTestEnvironment.SkipReason,
        SkipUnless = nameof(SqlServerTestEnvironment.Enabled),
        SkipType = typeof(SqlServerTestEnvironment))]
    public async Task ScriptIdempotente_DeveInstalarBancoVazioEPermitirReaplicacao()
    {
        await using var origem = fixture.CreateDbContext();
        var conexao = new SqlConnectionStringBuilder(origem.Database.GetConnectionString())
        {
            InitialCatalog = $"FichaDigitalScriptTests_{Guid.NewGuid():N}"
        };
        var options = new DbContextOptionsBuilder<FichaDigitalDbContext>()
            .UseSqlServer(conexao.ConnectionString).Options;
        await using var banco = new FichaDigitalDbContext(options);
        var criador = banco.GetService<IRelationalDatabaseCreator>();
        var cancellationToken = TestContext.Current.CancellationToken;
        await criador.CreateAsync(cancellationToken);
        try
        {
            var script = banco.GetService<IMigrator>().GenerateScript(
                options: MigrationsSqlGenerationOptions.Idempotent);
            await banco.Database.OpenConnectionAsync(cancellationToken);
            await banco.Database.ExecuteSqlRawAsync("SET QUOTED_IDENTIFIER ON", cancellationToken);
            for (var execucao = 0; execucao < 2; execucao++)
            {
                foreach (var lote in Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(lote))
                        await banco.Database.ExecuteSqlRawAsync(lote, cancellationToken);
                }
                Assert.Empty(await banco.Database.GetPendingMigrationsAsync(cancellationToken));
                Assert.Equal(banco.Database.GetMigrations(),
                    await banco.Database.GetAppliedMigrationsAsync(cancellationToken));
            }
        }
        finally
        {
            await banco.Database.CloseConnectionAsync();
            await banco.Database.EnsureDeletedAsync(CancellationToken.None);
        }
    }

    [Fact(
        Skip = SqlServerTestEnvironment.SkipReason,
        SkipUnless = nameof(SqlServerTestEnvironment.Enabled),
        SkipType = typeof(SqlServerTestEnvironment))]
    public async Task Migrations_DeveDeixarModeloSincronizado()
    {
        await using var dbContext = fixture.CreateDbContext();

        var migrationsPendentes = await dbContext.Database
            .GetPendingMigrationsAsync(TestContext.Current.CancellationToken);
        var migrationsAplicadas = await dbContext.Database
            .GetAppliedMigrationsAsync(TestContext.Current.CancellationToken);

        Assert.Empty(migrationsPendentes);
        Assert.Contains(
            "20260918021931_AddFichaOptimisticConcurrency",
            migrationsAplicadas);
    }

    [Fact(
        Skip = SqlServerTestEnvironment.SkipReason,
        SkipUnless = nameof(SqlServerTestEnvironment.Enabled),
        SkipType = typeof(SqlServerTestEnvironment))]
    public async Task ListarClientes_ComFiltroDaUltimaFicha_DeveFuncionar()
    {
        using var scope = fixture.CreateServiceScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ProfissionalUsuario>>();
        var sufixo = Guid.NewGuid().ToString("N");
        var profissional = new ProfissionalUsuario(
            "Profissional SQL Server",
            $"profissional-sql-{sufixo}@example.com");
        var criacaoProfissional = await userManager.CreateAsync(profissional);

        Assert.True(
            criacaoProfissional.Succeeded,
            string.Join(
                ", ",
                criacaoProfissional.Errors.Select(erro => erro.Description)));

        var cliente = new Cliente($"Cliente SQL Server {sufixo}");
        var ficha = new Ficha(
            cliente.Id,
            profissional.Id,
            profissional.NomeCompleto,
            TipoProcedimento.Tatuagem);

        dbContext.Clientes.Add(cliente);
        dbContext.Fichas.Add(ficha);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var consulta = scope.ServiceProvider.GetRequiredService<IConsultaClientes>();
        var hojeUtc = DateOnly.FromDateTime(DateTime.UtcNow);
        var resultado = await consulta.ListarAsync(
            new FiltroConsultaClientes(
                cliente.NomeReferencia,
                null,
                null,
                TipoProcedimento.Tatuagem,
                hojeUtc,
                hojeUtc,
                1,
                20),
            TestContext.Current.CancellationToken);

        var item = Assert.Single(resultado.Itens);
        Assert.Equal(cliente.Id, item.Id);
        Assert.Equal(TipoProcedimento.Tatuagem.ToString(),
            item.UltimaFicha?.TipoProcedimento);
    }

    [Fact(
        Skip = SqlServerTestEnvironment.SkipReason,
        SkipUnless = nameof(SqlServerTestEnvironment.Enabled),
        SkipType = typeof(SqlServerTestEnvironment))]
    public async Task AtualizarMesmaFicha_DeveDetectarConcorrencia()
    {
        Guid fichaId;

        await using (var preparacao = fixture.CreateDbContext())
        {
            var cliente = new Cliente(
                $"Cliente concorrente {Guid.NewGuid():N}");
            var ficha = new Ficha(cliente.Id);
            ficha.EnviarConvite();
            fichaId = ficha.Id;

            preparacao.Clientes.Add(cliente);
            preparacao.Fichas.Add(ficha);
            await preparacao.SaveChangesAsync(
                TestContext.Current.CancellationToken);
        }

        await using var primeiroContexto = fixture.CreateDbContext();
        await using var segundoContexto = fixture.CreateDbContext();
        var primeiraLeitura = await primeiroContexto.Fichas.SingleAsync(
            ficha => ficha.Id == fichaId,
            TestContext.Current.CancellationToken);
        var segundaLeitura = await segundoContexto.Fichas.SingleAsync(
            ficha => ficha.Id == fichaId,
            TestContext.Current.CancellationToken);

        primeiraLeitura.IniciarPreenchimento();
        segundaLeitura.IniciarPreenchimento();

        await primeiroContexto.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            segundoContexto.SaveChangesAsync(
                TestContext.Current.CancellationToken));
    }
}
