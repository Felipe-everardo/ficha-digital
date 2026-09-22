using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public interface IEmissaoConviteRepository
{
    Task<bool> ClienteExisteAsync(
        Guid clienteId,
        CancellationToken cancellationToken);

    Task AdicionarAsync(
        Ficha ficha,
        ConviteFicha convite,
        CancellationToken cancellationToken);
}
