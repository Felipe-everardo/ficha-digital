using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record ResultadoConclusaoProcedimento(
    StatusConclusaoProcedimento Resultado,
    Guid? FichaId = null,
    StatusFicha? StatusFicha = null,
    DateTimeOffset? ConcluidoEmUtc = null,
    string? EvidenciaHash = null);
