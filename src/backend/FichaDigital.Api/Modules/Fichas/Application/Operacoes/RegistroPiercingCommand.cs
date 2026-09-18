namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record RegistroPiercingCommand(
    string JoiaUtilizada,
    string AgulhaUtilizada,
    string LocalPerfuracao,
    string? Observacoes);
