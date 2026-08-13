using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record ResultadoAberturaConvite(
    StatusAberturaConvite Resultado,
    Guid? FichaId = null,
    StatusFicha? StatusFicha = null);
