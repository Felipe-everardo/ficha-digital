using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class EmitirConviteFichaService(
    FichaDigitalDbContext dbContext,
    GeradorTokenConvite geradorToken,
    TimeProvider timeProvider)
{
    private static readonly TimeSpan DuracaoConvite =
        TimeSpan.FromHours(1);

    public async Task<ConviteFichaEmitido?> EmitirAsync(
        Guid clienteId,
        CancellationToken cancellationToken)
    {
        var clienteExiste = await dbContext.Clientes
            .AnyAsync(
                cliente => cliente.Id == clienteId,
                cancellationToken);

        if (!clienteExiste)
        {
            return null;
        }

        var ficha = new Ficha(clienteId);
        var tokenGerado = geradorToken.Gerar();
        var expiraEmUtc = timeProvider
            .GetUtcNow()
            .Add(DuracaoConvite);

        var convite = new ConviteFicha(
            ficha.Id,
            tokenGerado.TokenHash,
            expiraEmUtc);

        ficha.EnviarConvite();

        dbContext.Fichas.Add(ficha);
        dbContext.ConvitesFicha.Add(convite);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ConviteFichaEmitido(
            ficha.Id,
            convite.Id,
            tokenGerado.TokenOriginal,
            convite.ExpiraEmUtc);
    }
}
