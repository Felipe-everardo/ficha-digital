namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record DadosPessoaisPreenchidosResponse(
    Guid FichaId,
    Guid ClienteId,
    string NomeParaExibicao,
    DateTimeOffset PreenchidosEmUtc);
