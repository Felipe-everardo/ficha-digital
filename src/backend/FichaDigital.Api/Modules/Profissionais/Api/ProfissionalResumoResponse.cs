namespace FichaDigital.Api.Modules.Profissionais.Api;

public sealed record ProfissionalResumoResponse(
    Guid Id,
    string NomeCompleto,
    IReadOnlyList<string> Especialidades);
