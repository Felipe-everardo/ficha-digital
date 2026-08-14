using System.Threading.RateLimiting;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Api;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using FichaDigital.Api.Modules.Profissionais.Api;
using FichaDigital.Api.Modules.Profissionais.Domain;
using FichaDigital.Api.Modules.Profissionais.Infrastructure.Provisionamento;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<FichaDigitalDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
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
        builder.Environment.IsEnvironment("Testing")
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
        builder.Environment.IsEnvironment("Testing")
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
});
builder.Services.AddAuthorization();
builder.Services.Configure<ProfissionalDesenvolvimentoOptions>(
    builder.Configuration.GetSection(
        ProfissionalDesenvolvimentoOptions.Secao));
builder.Services.AddScoped<ProvisionadorProfissionalDesenvolvimento>();
builder.Services.Configure<ProfissionalInicialOptions>(
    builder.Configuration.GetSection(
        ProfissionalInicialOptions.Secao));
builder.Services.AddScoped<ProvisionadorProfissionalInicial>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<GeradorTokenConvite>();
builder.Services.AddSingleton<CalculadorHashConteudo>();
builder.Services.AddScoped<EmitirConviteFichaService>();
builder.Services.AddScoped<AbrirConviteFichaService>();
builder.Services.AddScoped<ResponderQuestionarioSaudeService>();
builder.Services.AddScoped<AceitarTermoConsentimentoService>();
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

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Append(
            "X-Content-Type-Options",
            "nosniff");
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

        return Task.CompletedTask;
    });

    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program
{
}
