using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Modules.Fichas.Infrastructure;

public sealed class PersistenciaAtendimentoTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FalhaNaGravacao_DesfazFichaEAuditoria(bool falhaNaAuditoria)
    {
        using var factory = new FichaDigitalApiFactory();
        var id = await CriarFichaAsync(factory);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FichaDigitalDbContext>();
            var trigger = falhaNaAuditoria ? """
                CREATE TRIGGER FalhaSimulada AFTER INSERT ON RegistrosAuditoria
                BEGIN SELECT RAISE(ABORT, 'Falha simulada'); END;
                """ : """
                CREATE TRIGGER FalhaSimulada AFTER UPDATE ON Fichas
                BEGIN SELECT RAISE(ABORT, 'Falha simulada'); END;
                """;
            await db.Database.ExecuteSqlRawAsync(trigger, TestContext.Current.CancellationToken);
            var repository = scope.ServiceProvider.GetRequiredService<IAberturaConviteRepository>();
            var ficha = await repository.ObterFichaAsync(id, TestContext.Current.CancellationToken);
            ficha!.IniciarPreenchimento();
            await Assert.ThrowsAsync<DbUpdateException>(() => repository.SalvarAberturaAsync(
                ficha, "falha", TestContext.Current.CancellationToken));
        }

        using var verification = factory.Services.CreateScope();
        var persisted = verification.ServiceProvider.GetRequiredService<FichaDigitalDbContext>();
        Assert.Equal(StatusFicha.ConviteEnviado, (await persisted.Fichas.SingleAsync(
            item => item.Id == id, TestContext.Current.CancellationToken)).Status);
        Assert.Empty(await persisted.RegistrosAuditoria.ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EscritaConcorrente_PreservaPrimeiraAlteracaoSemAuditoriaDaSegunda()
    {
        using var factory = new FichaDigitalApiFactory();
        var id = await CriarFichaAsync(factory);
        using var firstScope = factory.Services.CreateScope();
        using var secondScope = factory.Services.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<IAberturaConviteRepository>();
        var second = secondScope.ServiceProvider.GetRequiredService<IAberturaConviteRepository>();
        var ficha1 = await first.ObterFichaAsync(id, TestContext.Current.CancellationToken);
        var ficha2 = await second.ObterFichaAsync(id, TestContext.Current.CancellationToken);
        ficha1!.IniciarPreenchimento();
        ficha2!.IniciarPreenchimento();

        await first.SalvarAberturaAsync(ficha1, "primeira", TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SalvarAberturaAsync(
            ficha2, "segunda", TestContext.Current.CancellationToken));

        using var verification = factory.Services.CreateScope();
        var db = verification.ServiceProvider.GetRequiredService<FichaDigitalDbContext>();
        var saved = await db.Fichas.SingleAsync(item => item.Id == id, TestContext.Current.CancellationToken);
        Assert.Equal(ficha1.Status, saved.Status);
        Assert.Equal(ficha1.VersaoConcorrencia, saved.VersaoConcorrencia);
        var audit = Assert.Single(await db.RegistrosAuditoria.ToListAsync(TestContext.Current.CancellationToken));
        Assert.Equal("primeira", audit.CorrelacaoId);
    }

    private static async Task<Guid> CriarFichaAsync(FichaDigitalApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FichaDigitalDbContext>();
        var cliente = new Cliente("Cliente fictício");
        var ficha = new Ficha(cliente.Id);
        ficha.EnviarConvite();
        db.Clientes.Add(cliente);
        db.Fichas.Add(ficha);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return ficha.Id;
    }
}
