using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Application;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Profissionais.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Infrastructure.SqlServer;

[Collection(SqlServerTestCollection.Name)]
public sealed class SqlServerCompatibilityTests(
    SqlServerContainerFixture fixture)
{
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

        var consulta = new ConsultaClientes(dbContext);
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
