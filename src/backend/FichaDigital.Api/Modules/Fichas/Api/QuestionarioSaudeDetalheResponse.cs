namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record QuestionarioSaudeDetalheResponse(
    int Versao,
    bool TemDiabetes,
    string? TipoDiabetes,
    bool PossuiPressaoAlta,
    bool TemAlergia,
    string? DescricaoAlergia,
    bool PossuiCondicaoCardiaca,
    bool TemEpilepsia,
    bool TemHemofilia,
    bool UsaMarcaPasso,
    bool EstaGravidaOuAmamentando,
    DateTimeOffset RespondidoEmUtc);
