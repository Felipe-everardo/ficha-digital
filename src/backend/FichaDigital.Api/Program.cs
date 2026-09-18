using FichaDigital.Api.Infrastructure.Auditing;
using FichaDigital.Api.Infrastructure.Configuration;
using FichaDigital.Api.Infrastructure.Idempotency;
using FichaDigital.Api.Infrastructure.Web;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsEnvironment("E2E"))
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
}

builder.Services.AddFichaDigitalApi(
    builder.Configuration,
    builder.Environment);
builder.Services.AddFichaDigitalPersistence(builder.Configuration);
builder.Services.AddFichaDigitalSecurity(builder.Environment);
builder.Services.AddFichaDigitalApplication(builder.Configuration);

var app = builder.Build();

await app.InitializeFichaDigitalAsync();

if (!app.Environment.IsDevelopment() &&
    !app.Environment.IsEnvironment("E2E"))
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

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
        new HealthCheckOptions { Predicate = _ => false })
    .AllowAnonymous();
app.MapHealthChecks(
        "/health/ready",
        new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("ready")
        })
    .AllowAnonymous();
app.MapFallbackToFile("index.html").AllowAnonymous();

app.Run();

public partial class Program
{
}
