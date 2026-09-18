using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Infrastructure.Idempotency;

public sealed class IdempotenciaMiddleware(
    RequestDelegate next,
    IDataProtectionProvider dataProtectionProvider,
    ILogger<IdempotenciaMiddleware> logger)
{
    public const string NomeCabecalho = "Idempotency-Key";

    private readonly IDataProtector _protector = dataProtectionProvider
        .CreateProtector("FichaDigital.Idempotencia.Resposta.v1");

    public async Task InvokeAsync(
        HttpContext context,
        Persistence.FichaDigitalDbContext dbContext,
        TimeProvider timeProvider)
    {
        if (!EhMetodoMutavel(context.Request.Method) ||
            !context.Request.Headers.TryGetValue(
                NomeCabecalho,
                out var valoresChave))
        {
            await next(context);
            return;
        }

        var chave = valoresChave.ToString().Trim();
        if (!Guid.TryParse(chave, out _) || chave.Length > 80)
        {
            await Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Chave de idempotência inválida.",
                    detail: "Envie um UUID válido no cabeçalho Idempotency-Key.")
                .ExecuteAsync(context);
            return;
        }

        var escopo = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? "publico";
        var fingerprint = await CalcularFingerprintAsync(context.Request);
        var agora = timeProvider.GetUtcNow();
        var existente = await dbContext.Set<RequisicaoIdempotente>()
            .SingleOrDefaultAsync(
                item => item.Escopo == escopo && item.Chave == chave,
                context.RequestAborted);

        if (existente is not null && existente.ExpiraEmUtc <= agora)
        {
            dbContext.Remove(existente);
            await dbContext.SaveChangesAsync(context.RequestAborted);
            existente = null;
        }

        if (existente is not null)
        {
            await ResponderRepeticaoAsync(context, existente, fingerprint);
            return;
        }

        var requisicao = new RequisicaoIdempotente(
            escopo,
            chave,
            fingerprint,
            context.Request.Method,
            context.Request.Path,
            agora);
        dbContext.Add(requisicao);

        try
        {
            await dbContext.SaveChangesAsync(context.RequestAborted);
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();
            existente = await dbContext.Set<RequisicaoIdempotente>()
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.Escopo == escopo && item.Chave == chave,
                    context.RequestAborted);

            if (existente is not null)
            {
                await ResponderRepeticaoAsync(
                    context,
                    existente,
                    fingerprint);
                return;
            }

            throw;
        }

        var corpoOriginal = context.Response.Body;
        await using var corpoTemporario = new MemoryStream();
        context.Response.Body = corpoTemporario;

        try
        {
            await next(context);

            corpoTemporario.Position = 0;
            var textoResposta = await new StreamReader(
                    corpoTemporario,
                    Encoding.UTF8,
                    leaveOpen: true)
                .ReadToEndAsync(context.RequestAborted);

            if (context.Response.StatusCode is >= 200 and < 300)
            {
                requisicao.Concluir(
                    context.Response.StatusCode,
                    context.Response.ContentType,
                    _protector.Protect(textoResposta),
                    context.Response.Headers.Location.ToString(),
                    timeProvider.GetUtcNow());
                await dbContext.SaveChangesAsync(context.RequestAborted);
            }
            else
            {
                dbContext.Remove(requisicao);
                await dbContext.SaveChangesAsync(context.RequestAborted);
            }

            corpoTemporario.Position = 0;
            context.Response.Body = corpoOriginal;
            await corpoTemporario.CopyToAsync(
                corpoOriginal,
                context.RequestAborted);
        }
        catch
        {
            context.Response.Body = corpoOriginal;

            try
            {
                dbContext.ChangeTracker.Clear();
                dbContext.Remove(requisicao);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception limpezaException)
            {
                logger.LogWarning(
                    limpezaException,
                    "Não foi possível remover uma reserva de idempotência incompleta.");
            }

            throw;
        }
    }

    private async Task ResponderRepeticaoAsync(
        HttpContext context,
        RequisicaoIdempotente existente,
        string fingerprint)
    {
        if (!string.Equals(
                existente.Fingerprint,
                fingerprint,
                StringComparison.Ordinal))
        {
            await Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Chave de idempotência reutilizada.",
                    detail: "Use uma nova chave para uma requisição com dados diferentes.")
                .ExecuteAsync(context);
            return;
        }

        if (!existente.Concluida ||
            existente.StatusCode is null ||
            existente.CorpoRespostaProtegido is null)
        {
            await Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Operação em andamento.",
                    detail: "A mesma operação já está sendo processada. Aguarde e tente novamente.")
                .ExecuteAsync(context);
            return;
        }

        try
        {
            var corpo = _protector.Unprotect(
                existente.CorpoRespostaProtegido);
            context.Response.StatusCode = existente.StatusCode.Value;
            context.Response.ContentType = existente.TipoConteudo;
            if (!string.IsNullOrWhiteSpace(existente.Localizacao))
            {
                context.Response.Headers.Location = existente.Localizacao;
            }

            context.Response.Headers["Idempotency-Replayed"] = "true";
            await context.Response.WriteAsync(
                corpo,
                context.RequestAborted);
        }
        catch (CryptographicException exception)
        {
            logger.LogError(
                exception,
                "Não foi possível recuperar uma resposta idempotente.");
            await Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Não foi possível repetir a resposta.",
                    detail: "Envie a operação novamente com uma nova chave.")
                .ExecuteAsync(context);
        }
    }

    private static bool EhMetodoMutavel(string metodo)
    {
        return HttpMethods.IsPost(metodo) ||
            HttpMethods.IsPut(metodo) ||
            HttpMethods.IsPatch(metodo) ||
            HttpMethods.IsDelete(metodo);
    }

    private static async Task<string> CalcularFingerprintAsync(
        HttpRequest request)
    {
        request.EnableBuffering();
        using var leitor = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        var corpo = await leitor.ReadToEndAsync(request.HttpContext.RequestAborted);
        request.Body.Position = 0;
        var conteudo = string.Join(
            '\n',
            request.Method,
            request.Path.Value,
            request.QueryString.Value,
            corpo);

        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(conteudo)));
    }
}
