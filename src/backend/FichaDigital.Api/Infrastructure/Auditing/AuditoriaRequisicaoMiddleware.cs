using System.Security.Claims;
using Microsoft.AspNetCore.Routing;

namespace FichaDigital.Api.Infrastructure.Auditing;

public sealed class AuditoriaRequisicaoMiddleware(
    RequestDelegate next,
    ILogger<AuditoriaRequisicaoMiddleware> logger)
{
    public async Task InvokeAsync(
        HttpContext context,
        Persistence.FichaDigitalDbContext dbContext,
        TimeProvider timeProvider)
    {
        await next(context);

        if (context.Response.StatusCode is < 200 or >= 400 ||
            !ObterProfissionalId(context.User, out var profissionalId) ||
            !TentarIdentificarRecurso(
                context,
                out var recurso,
                out var recursoId))
        {
            return;
        }

        try
        {
            var padraoRota = (context.GetEndpoint() as RouteEndpoint)?
                .RoutePattern.RawText ?? context.Request.Path.Value ?? "/";
            dbContext.Set<RegistroAuditoria>().Add(new RegistroAuditoria(
                profissionalId,
                $"{context.Request.Method} {padraoRota}",
                recurso,
                recursoId,
                context.TraceIdentifier,
                timeProvider.GetUtcNow()));
            await dbContext.SaveChangesAsync(context.RequestAborted);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Falha ao registrar auditoria da requisição {CorrelationId}.",
                context.TraceIdentifier);
        }
    }

    private static bool ObterProfissionalId(
        ClaimsPrincipal principal,
        out Guid profissionalId)
    {
        return Guid.TryParse(
            principal.FindFirstValue(ClaimTypes.NameIdentifier),
            out profissionalId);
    }

    private static bool TentarIdentificarRecurso(
        HttpContext context,
        out string recurso,
        out Guid? recursoId)
    {
        var caminho = context.Request.Path.Value ?? string.Empty;
        if (caminho.StartsWith("/api/fichas", StringComparison.OrdinalIgnoreCase))
        {
            recurso = "Ficha";
            recursoId = ObterIdDaRota(context, "fichaId");
            return true;
        }

        if (caminho.StartsWith("/api/clientes", StringComparison.OrdinalIgnoreCase))
        {
            recurso = "Cliente";
            recursoId = ObterIdDaRota(context, "clienteId");
            return true;
        }

        recurso = string.Empty;
        recursoId = null;
        return false;
    }

    private static Guid? ObterIdDaRota(HttpContext context, string chave)
    {
        return Guid.TryParse(
            context.Request.RouteValues[chave]?.ToString(),
            out var id)
                ? id
                : null;
    }
}
