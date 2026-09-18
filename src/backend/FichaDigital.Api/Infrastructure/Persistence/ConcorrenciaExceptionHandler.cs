using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Infrastructure.Persistence;

public sealed class ConcorrenciaExceptionHandler
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateConcurrencyException)
        {
            return false;
        }

        await Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "A ficha foi atualizada por outra operação.",
                detail: "Recarregue os dados e tente novamente.")
            .ExecuteAsync(httpContext);

        return true;
    }
}
