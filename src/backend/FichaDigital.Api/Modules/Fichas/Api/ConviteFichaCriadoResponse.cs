namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record ConviteFichaCriadoResponse(
    Guid FichaId,
    Guid ConviteId,
    string LinkPreenchimento,
    DateTimeOffset ExpiraEmUtc);
