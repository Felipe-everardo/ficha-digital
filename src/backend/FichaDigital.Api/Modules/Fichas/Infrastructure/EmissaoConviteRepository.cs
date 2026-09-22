using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure;

internal sealed class EmissaoConviteRepository(
    FichaDigitalDbContext dbContext) : IEmissaoConviteRepository
{
    public Task<bool> ClienteExisteAsync(
        Guid clienteId,
        CancellationToken cancellationToken)
    {
        return dbContext.Clientes
            .AsNoTracking()
            .AnyAsync(
                cliente => cliente.Id == clienteId,
                cancellationToken);
    }

    public async Task AdicionarAsync(
        Ficha ficha,
        ConviteFicha convite,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ficha);
        ArgumentNullException.ThrowIfNull(convite);

        dbContext.Fichas.Add(ficha);
        dbContext.ConvitesFicha.Add(convite);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
