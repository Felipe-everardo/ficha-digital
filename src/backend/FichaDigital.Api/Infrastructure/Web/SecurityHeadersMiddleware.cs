namespace FichaDigital.Api.Infrastructure.Web;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append(
                "X-Content-Type-Options",
                "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("Referrer-Policy", "no-referrer");
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

        await next(context);
    }
}
