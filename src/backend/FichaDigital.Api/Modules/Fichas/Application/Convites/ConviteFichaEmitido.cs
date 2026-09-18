namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record ConviteFichaEmitido(
    Guid FichaId,
    Guid ConviteId,
    string TokenOriginal,
    DateTimeOffset ExpiraEmUtc);
