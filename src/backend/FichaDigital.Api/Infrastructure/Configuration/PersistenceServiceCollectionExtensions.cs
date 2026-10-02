using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Application;
using FichaDigital.Api.Modules.Clientes.Infrastructure;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Infrastructure.Configuration;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddFichaDigitalPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<FichaDigitalDbContext>(options =>
        {
            var connectionString = configuration
                .GetConnectionString("DefaultConnection");

            if (string.Equals(
                    configuration["DatabaseProvider"],
                    "Sqlite",
                    StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
                return;
            }

            options.UseSqlServer(
                connectionString,
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(15),
                    errorNumbersToAdd: null));
        });
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<
            IEmissaoConviteRepository,
            EmissaoConviteRepository>();

        services.AddScoped<IAberturaConviteRepository, AberturaConviteRepository>();
        services.AddScoped<IDadosPessoaisRepository, DadosPessoaisRepository>();
        services.AddScoped<IQuestionarioSaudeRepository, QuestionarioSaudeRepository>();
        services.AddScoped<IConsentimentoRepository, ConsentimentoRepository>();
        services.AddScoped<IRevisaoFichaRepository, RevisaoFichaRepository>();
        services.AddScoped<IConclusaoProcedimentoRepository, ConclusaoProcedimentoRepository>();
        services.AddScoped<IConsultaClientes, ConsultaClientes>();
        services.AddScoped<IConsultaListagemFichas, ConsultaListagemFichas>();
        services.AddScoped<IConsultaDetalheFicha, ConsultaDetalheFicha>();

        return services;
    }
}
