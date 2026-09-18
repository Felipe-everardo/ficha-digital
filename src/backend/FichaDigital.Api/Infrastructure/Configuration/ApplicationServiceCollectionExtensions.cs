using FichaDigital.Api.Infrastructure.Auditing;
using FichaDigital.Api.Infrastructure.Idempotency;
using FichaDigital.Api.Modules.Clientes.Application;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Infrastructure;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using FichaDigital.Api.Modules.Profissionais.Domain;
using FichaDigital.Api.Modules.Profissionais.Infrastructure.Provisionamento;

namespace FichaDigital.Api.Infrastructure.Configuration;

internal static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddFichaDigitalApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ProfissionalDesenvolvimentoOptions>(
            configuration.GetSection(
                ProfissionalDesenvolvimentoOptions.Secao));
        services.AddScoped<ProvisionadorProfissionalDesenvolvimento>();
        services.Configure<ProfissionalInicialOptions>(
            configuration.GetSection(ProfissionalInicialOptions.Secao));
        services.AddScoped<ProvisionadorProfissionalInicial>();
        services.AddOptions<EstudioOptions>()
            .BindConfiguration(EstudioOptions.Secao)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddHostedService<LimpezaIdempotenciaService>();
        services.AddSingleton<GeradorTokenConvite>();
        services.AddSingleton<CalculadorHashConteudo>();
        services.AddSingleton<ResolvedorModeloFicha>();

        services.AddScoped<CriarClienteService>();
        services.AddScoped<ConsultaClientes>();
        services.AddScoped<AuditoriaService>();
        services.AddScoped<ListarFichasService>();
        services.AddScoped<ObterFichaDetalheService>();
        services.AddScoped<EmitirConviteFichaService>();
        services.AddScoped<AbrirConviteFichaService>();
        services.AddScoped<PreencherDadosPessoaisService>();
        services.AddScoped<ResponderQuestionarioSaudeService>();
        services.AddScoped<AceitarTermoConsentimentoService>();
        services.AddScoped<RevisarFichaService>();
        services.AddScoped<ConcluirProcedimentoService>();

        return services;
    }
}
