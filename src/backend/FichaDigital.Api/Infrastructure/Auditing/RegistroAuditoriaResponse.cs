namespace FichaDigital.Api.Infrastructure.Auditing;

public sealed record RegistroAuditoriaResponse(
    Guid Id,
    Guid? ProfissionalId,
    string ProfissionalNome,
    string Origem,
    string Acao,
    string Recurso,
    Guid? RecursoId,
    string CorrelacaoId,
    DateTimeOffset OcorreuEmUtc);
