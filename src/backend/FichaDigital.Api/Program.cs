using System.Threading.RateLimiting;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Api;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<FichaDigitalDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
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
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();

public partial class Program
{
}
