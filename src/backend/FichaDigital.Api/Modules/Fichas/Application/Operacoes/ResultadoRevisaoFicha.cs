using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record ResultadoRevisaoFicha(
    StatusRevisaoFicha Resultado,
    Guid? FichaId = null,
    StatusFicha? StatusFicha = null,
    DateTimeOffset? RevisadaEmUtc = null);
