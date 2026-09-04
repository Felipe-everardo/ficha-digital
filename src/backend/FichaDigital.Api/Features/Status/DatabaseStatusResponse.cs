namespace FichaDigital.Api.Features.Status;

public sealed record DatabaseStatusResponse(
    string Application,
    string Database,
    string Message,
    DateTimeOffset CheckedAtUtc);
