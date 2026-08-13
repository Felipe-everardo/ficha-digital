namespace FichaDigital.Api.Modules.Fichas.Infrastructure.Security;

public sealed record TokenConviteGerado(
    string TokenOriginal,
    string TokenHash);
