using FichaDigital.Api.Features.Status;
using FichaDigital.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;

namespace FichaDigital.Api.Infrastructure.Configuration;

internal static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddFichaDigitalApi(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddControllersWithViews();

        var dataProtection = services.AddDataProtection()
            .SetApplicationName("FichaDigital");

        if (environment.IsEnvironment("E2E"))
        {
            dataProtection.UseEphemeralDataProtectionProvider();
        }
        else
        {
            var caminhoChaves = configuration["DataProtection:KeysPath"];
            if (!string.IsNullOrWhiteSpace(caminhoChaves))
            {
                dataProtection.PersistKeysToFileSystem(
                    new DirectoryInfo(caminhoChaves));
            }
        }

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["correlationId"] =
                    context.HttpContext.TraceIdentifier;
            };
        });
        services.AddExceptionHandler<ConcorrenciaExceptionHandler>();
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

        return services;
    }
}
