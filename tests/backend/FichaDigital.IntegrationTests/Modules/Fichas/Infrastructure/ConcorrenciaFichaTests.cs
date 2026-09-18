using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Modules.Fichas.Infrastructure;

public sealed class ConcorrenciaFichaTests
{
    [Fact]
    public async Task AtualizarMesmaVersaoEmDoisContextos_DeveDetectarConflito()
    {
        using var factory = new FichaDigitalApiFactory();
        var fichaId = await CriarFichaComConviteAsync(factory);

        using var primeiroEscopo = factory.Services.CreateScope();
        using var segundoEscopo = factory.Services.CreateScope();
        var primeiroContexto = primeiroEscopo.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var segundoContexto = segundoEscopo.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
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

    private static async Task<Guid> CriarFichaComConviteAsync(
        FichaDigitalApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var cliente = new Cliente("Cliente concorrente");
        var ficha = new Ficha(cliente.Id);
        ficha.EnviarConvite();

        dbContext.Clientes.Add(cliente);
        dbContext.Fichas.Add(ficha);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        return ficha.Id;
    }
}
