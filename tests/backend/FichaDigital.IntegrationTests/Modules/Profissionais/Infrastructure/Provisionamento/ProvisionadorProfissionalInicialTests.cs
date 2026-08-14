using FichaDigital.Api.Modules.Profissionais.Domain;
using FichaDigital.Api.Modules.Profissionais.Infrastructure.Provisionamento;
using FichaDigital.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FichaDigital.IntegrationTests.Modules.Profissionais.Infrastructure.Provisionamento;

public sealed class ProvisionadorProfissionalInicialTests
{
    private const string Email = "profissional.inicial@example.com";
    private const string SenhaInicial = "Senha-Inicial-123!";

    [Fact]
    public async Task Provisionar_ComConfiguracaoHabilitada_DeveCriarProfissional()
    {
        using var factory = new FichaDigitalApiFactory();
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ProfissionalUsuario>>();
        var provisionador = CriarProvisionador(
            userManager,
            SenhaInicial);

        await provisionador.ProvisionarAsync();

        var profissional = await userManager.FindByEmailAsync(Email);

        Assert.NotNull(profissional);
        Assert.Equal("Profissional Inicial", profissional.NomeCompleto);
        Assert.True(profissional.EmailConfirmed);
        Assert.True(await userManager.CheckPasswordAsync(
            profissional,
            SenhaInicial));
    }

    [Fact]
    public async Task Provisionar_QuandoProfissionalJaExiste_NaoDeveAlterarSenha()
    {
        using var factory = new FichaDigitalApiFactory();
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ProfissionalUsuario>>();
        var primeiroProvisionador = CriarProvisionador(
            userManager,
            SenhaInicial);

        await primeiroProvisionador.ProvisionarAsync();

        var segundoProvisionador = CriarProvisionador(
            userManager,
            "Outra-Senha-456!");

        await segundoProvisionador.ProvisionarAsync();

        var profissional = await userManager.FindByEmailAsync(Email);

        Assert.NotNull(profissional);
        Assert.True(await userManager.CheckPasswordAsync(
            profissional,
            SenhaInicial));
        Assert.False(await userManager.CheckPasswordAsync(
            profissional,
            "Outra-Senha-456!"));
    }

    [Fact]
    public async Task Provisionar_Desabilitado_NaoDeveExigirCredenciais()
    {
        using var factory = new FichaDigitalApiFactory();
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ProfissionalUsuario>>();
        var options = Options.Create(new ProfissionalInicialOptions
        {
            Habilitado = false
        });
        var provisionador = new ProvisionadorProfissionalInicial(
            userManager,
            options,
            NullLogger<ProvisionadorProfissionalInicial>.Instance);

        await provisionador.ProvisionarAsync();

        Assert.Null(await userManager.FindByEmailAsync(Email));
    }

    private static ProvisionadorProfissionalInicial CriarProvisionador(
        UserManager<ProfissionalUsuario> userManager,
        string senha)
    {
        var options = Options.Create(new ProfissionalInicialOptions
        {
            Habilitado = true,
            NomeCompleto = " Profissional Inicial ",
            Email = $" {Email} ",
            Senha = senha
        });

        return new ProvisionadorProfissionalInicial(
            userManager,
            options,
            NullLogger<ProvisionadorProfissionalInicial>.Instance);
    }
}
