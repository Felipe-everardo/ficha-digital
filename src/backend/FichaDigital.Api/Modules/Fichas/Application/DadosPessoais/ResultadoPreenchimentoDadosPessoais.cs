namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record ResultadoPreenchimentoDadosPessoais(
    StatusPreenchimentoDadosPessoais Resultado,
    Guid? FichaId = null,
    Guid? ClienteId = null,
    string? NomeParaExibicao = null,
    DateTimeOffset? PreenchidosEmUtc = null);
