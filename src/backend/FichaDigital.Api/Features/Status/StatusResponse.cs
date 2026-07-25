namespace FichaDigital.Api.Features.Status;

public sealed record StatusResponse(
    string Application,
    string Message,
    string Version,
    DateTimeOffset CheckedAtUtc);
