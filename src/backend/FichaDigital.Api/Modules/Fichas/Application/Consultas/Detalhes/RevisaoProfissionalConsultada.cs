namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record RevisaoProfissionalConsultada(
    string ProfissionalNome,
    bool DadosDaFichaConferidos,
    DateTimeOffset RevisadaEmUtc);
