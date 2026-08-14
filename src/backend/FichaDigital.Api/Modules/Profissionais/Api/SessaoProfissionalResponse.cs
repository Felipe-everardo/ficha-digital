namespace FichaDigital.Api.Modules.Profissionais.Api;

public sealed record SessaoProfissionalResponse(
    Guid ProfissionalId,
    string NomeCompleto,
    string Email);
