using System.Threading.RateLimiting;
using FichaDigital.Api.Features.Status;
using FichaDigital.Api.Infrastructure.Auditing;
using FichaDigital.Api.Infrastructure.Idempotency;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Application;
using FichaDigital.Api.Modules.Fichas.Api;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Infrastructure;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using FichaDigital.Api.Modules.Profissionais.Api;
using FichaDigital.Api.Modules.Profissionais.Domain;
using FichaDigital.Api.Modules.Profissionais.Infrastructure.Provisionamento;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsEnvironment("E2E"))
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
}

builder.Services.AddControllersWithViews();
if (builder.Environment.IsEnvironment("E2E"))
{
    builder.Services.AddDataProtection()
        .UseEphemeralDataProtectionProvider();
}
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["correlationId"] =
            context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);
builder.Services.AddDbContext<FichaDigitalDbContext>(options =>
{
    var connectionString = builder.Configuration
        .GetConnectionString("DefaultConnection");

    if (string.Equals(
            builder.Configuration["DatabaseProvider"],
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
builder.Services
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
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "FichaDigital.Profissional";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ||
        builder.Environment.IsEnvironment("Testing") ||
        builder.Environment.IsEnvironment("E2E")
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "FichaDigital.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ||
        builder.Environment.IsEnvironment("Testing") ||
        builder.Environment.IsEnvironment("E2E")
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
});
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.Configure<ProfissionalDesenvolvimentoOptions>(
    builder.Configuration.GetSection(
        ProfissionalDesenvolvimentoOptions.Secao));
builder.Services.AddScoped<ProvisionadorProfissionalDesenvolvimento>();
builder.Services.Configure<ProfissionalInicialOptions>(
    builder.Configuration.GetSection(
        ProfissionalInicialOptions.Secao));
builder.Services.AddScoped<ProvisionadorProfissionalInicial>();
builder.Services.AddOptions<EstudioOptions>()
    .BindConfiguration(EstudioOptions.Secao)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<LimpezaIdempotenciaService>();
builder.Services.AddSingleton<GeradorTokenConvite>();
builder.Services.AddSingleton<CalculadorHashConteudo>();
builder.Services.AddSingleton<ResolvedorModeloFicha>();
builder.Services.AddScoped<ConsultaClientes>();
builder.Services.AddScoped<AuditoriaService>();
builder.Services.AddScoped<ConsultaFichas>();
builder.Services.AddScoped<EmitirConviteFichaService>();
builder.Services.AddScoped<AbrirConviteFichaService>();
builder.Services.AddScoped<PreencherDadosPessoaisService>();
builder.Services.AddScoped<ResponderQuestionarioSaudeService>();
builder.Services.AddScoped<AceitarTermoConsentimentoService>();
builder.Services.AddScoped<RevisarFichaService>();
builder.Services.AddScoped<ConcluirProcedimentoService>();
builder.Services.AddRateLimiter(options =>
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
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "ip-desconhecido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy(
        PoliticasRateLimitingAutenticacao.LoginProfissionais,
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "ip-desconhecido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var aplicarMigrationsAoIniciar = app.Configuration.GetValue<bool>(
        "DatabaseInitialization:ApplyMigrationsOnStartup");

    if (app.Environment.IsEnvironment("E2E"))
    {
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
    else if (!app.Environment.IsEnvironment("Testing") &&
        aplicarMigrationsAoIniciar)
    {
        var dbContext = scope.ServiceProvider
            .GetRequiredService<FichaDigitalDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    if (app.Environment.IsDevelopment())
    {
        var provisionadorDesenvolvimento = scope.ServiceProvider
            .GetRequiredService<ProvisionadorProfissionalDesenvolvimento>();

        await provisionadorDesenvolvimento.ProvisionarAsync();
    }

    var provisionadorInicial = scope.ServiceProvider
        .GetRequiredService<ProvisionadorProfissionalInicial>();

    await provisionadorInicial.ProvisionarAsync();
}

if (!app.Environment.IsDevelopment() &&
    !app.Environment.IsEnvironment("E2E"))
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers
        .TryGetValue("X-Correlation-ID", out var recebido) &&
        !string.IsNullOrWhiteSpace(recebido)
            ? recebido.ToString()[..Math.Min(recebido.ToString().Length, 100)]
            : context.TraceIdentifier;
    context.TraceIdentifier = correlationId;
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        return Task.CompletedTask;
    });

    await next();
});

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Append(
            "X-Content-Type-Options",
            "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append(
            "Referrer-Policy",
            "no-referrer");
        context.Response.Headers.Append(
            "Permissions-Policy",
            "camera=(), microphone=(), geolocation=()");
        context.Response.Headers.Append(
            "Content-Security-Policy",
            "default-src 'self'; base-uri 'self'; frame-ancestors 'none'; " +
            "form-action 'self'; object-src 'none'; img-src 'self' data:; " +
            "font-src 'self'; script-src 'self'; style-src 'self'; " +
            "connect-src 'self'; upgrade-insecure-requests");

        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.Headers.CacheControl = "no-store";
        }

        return Task.CompletedTask;
    });

    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditoriaRequisicaoMiddleware>();
app.UseMiddleware<IdempotenciaMiddleware>();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks(
        "/health/live",
        new HealthCheckOptions
        {
            Predicate = _ => false
        })
    .AllowAnonymous();
app.MapHealthChecks(
        "/health/ready",
        new HealthCheckOptions
        {
            Predicate = healthCheck =>
                healthCheck.Tags.Contains("ready")
        })
    .AllowAnonymous();
app.MapFallbackToFile("index.html").AllowAnonymous();

app.Run();

public partial class Program
{
}
