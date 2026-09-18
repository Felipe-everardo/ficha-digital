namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record RevisaoProfissionalResponse(
    string ProfissionalNome,
    bool DadosDaFichaConferidos,
    DateTimeOffset RevisadaEmUtc);
