using System.Threading.RateLimiting;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Api;
using FichaDigital.Api.Modules.Profissionais.Api;
using FichaDigital.Api.Modules.Profissionais.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;

namespace FichaDigital.Api.Infrastructure.Configuration;

internal static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddFichaDigitalSecurity(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        services
            .AddIdentity<ProfissionalUsuario, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 12;
                options.Password.RequiredUniqueChars = 4;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<FichaDigitalDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "FichaDigital.Profissional";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicyFor(environment);
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = true;
        });
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "FichaDigital.Antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicyFor(environment);
        });
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
        services.AddFichaDigitalRateLimiting();

        return services;
    }

    private static IServiceCollection AddFichaDigitalRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, _) =>
            {
                await Results.Problem(
                        statusCode: StatusCodes.Status429TooManyRequests,
                        title: "Muitas tentativas.",
                        detail: "Aguarde antes de tentar novamente.")
                    .ExecuteAsync(context.HttpContext);
            };

            options.AddPolicy(
                PoliticasRateLimitingFichas.ConvitesPublicos,
                CriarLimitadorPorIp);
            options.AddPolicy(
                PoliticasRateLimitingAutenticacao.LoginProfissionais,
                CriarLimitadorPorIp);
        });

        return services;
    }

    private static RateLimitPartition<string> CriarLimitadorPorIp(
        HttpContext httpContext)
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "ip-desconhecido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    }

    private static CookieSecurePolicy CookieSecurePolicyFor(
        IHostEnvironment environment)
    {
        return environment.IsDevelopment() ||
            environment.IsEnvironment("Testing") ||
            environment.IsEnvironment("E2E")
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
    }
}
