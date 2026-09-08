using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using FichaDigital.Api.Modules.Profissionais.Domain;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FichaDigital.IntegrationTests.Modules.Fichas.Application;

public sealed class EmitirConviteFichaServiceTests
{
    [Fact]
    public async Task Emitir_ComClienteExistente_DevePersistirFichaEConvite()
    {
        var instanteEmissao = DateTimeOffset.UtcNow;
        using var factory = new FichaDigitalApiFactory(
            new FixedTimeProvider(instanteEmissao));
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var service = scope.ServiceProvider
            .GetRequiredService<EmitirConviteFichaService>();
        var geradorToken = scope.ServiceProvider
            .GetRequiredService<GeradorTokenConvite>();
        var profissional = await CriarProfissionalAsync(scope);

        var cliente = CriarCliente();
        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var resultado = await service.EmitirAsync(
            cliente.Id,
            profissional.Id,
            profissional.NomeCompleto,
            TipoProcedimento.Tatuagem,
            TestContext.Current.CancellationToken);

        Assert.NotNull(resultado);
        Assert.NotEqual(Guid.Empty, resultado.FichaId);
        Assert.NotEqual(Guid.Empty, resultado.ConviteId);
        Assert.NotEmpty(resultado.TokenOriginal);
        Assert.Equal(instanteEmissao.AddHours(1), resultado.ExpiraEmUtc);

        dbContext.ChangeTracker.Clear();

        var ficha = await dbContext.Fichas
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == resultado.FichaId,
                TestContext.Current.CancellationToken);

        var convite = await dbContext.ConvitesFicha
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == resultado.ConviteId,
                TestContext.Current.CancellationToken);

        Assert.Equal(cliente.Id, ficha.ClienteId);
        Assert.Equal(profissional.Id, ficha.ProfissionalResponsavelId);
        Assert.Equal(profissional.NomeCompleto, ficha.ProfissionalResponsavelNome);
        Assert.Equal(TipoProcedimento.Tatuagem, ficha.TipoProcedimento);
        Assert.Equal(StatusFicha.ConviteEnviado, ficha.Status);
        Assert.Equal(ficha.Id, convite.FichaId);
        Assert.Equal(
            geradorToken.CalcularHash(resultado.TokenOriginal),
            convite.TokenHash);
        Assert.Equal(resultado.ExpiraEmUtc, convite.ExpiraEmUtc);
    }

    [Fact]
    public async Task Emitir_ComClienteInexistente_DeveRetornarNullENaoPersistirDados()
    {
        using var factory = new FichaDigitalApiFactory();
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        var service = scope.ServiceProvider
            .GetRequiredService<EmitirConviteFichaService>();
        var profissional = await CriarProfissionalAsync(scope);

        var resultado = await service.EmitirAsync(
            Guid.NewGuid(),
            profissional.Id,
            profissional.NomeCompleto,
            TipoProcedimento.Piercing,
            TestContext.Current.CancellationToken);

        Assert.Null(resultado);
        Assert.Empty(await dbContext.Fichas.ToListAsync(
            TestContext.Current.CancellationToken));
        Assert.Empty(await dbContext.ConvitesFicha.ToListAsync(
            TestContext.Current.CancellationToken));
    }

    private static Cliente CriarCliente()
    {
        return new Cliente(
            "Ana Silva",
            "Ana",
            "ela/dela",
            new DateOnly(1995, 6, 15),
            "(21) 99999-9999",
            "ana@example.com");
    }

    private static async Task<ProfissionalUsuario> CriarProfissionalAsync(
        IServiceScope scope)
    {
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ProfissionalUsuario>>();
        var profissional = new ProfissionalUsuario(
            "Profissional Responsável",
            $"responsavel-{Guid.NewGuid():N}@example.com");
        var resultado = await userManager.CreateAsync(profissional);

        Assert.True(resultado.Succeeded);
        return profissional;
    }
}
