namespace FichaDigital.Api.Infrastructure.Web;

public sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    ILogger<CorrelationIdMiddleware> logger)
{
    private const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers
            .TryGetValue(HeaderName, out var recebido) &&
            !string.IsNullOrWhiteSpace(recebido)
                ? recebido.ToString()[..Math.Min(recebido.ToString().Length, 100)]
                : context.TraceIdentifier;
        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await next(context);
        }
    }
}
